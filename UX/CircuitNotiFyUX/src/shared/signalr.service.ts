import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {
  private hubConnection!: signalR.HubConnection;

  startConnection(): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('https://localhost:7003/notificationHub', {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets
    })
    .withAutomaticReconnect()
    .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connection established.'))
      .catch(err => console.error('Error while starting SignalR connection:', err));
  }

  addServiceStateListener(callback: (message: any) => void): void {
    this.hubConnection.on('SendServiceState', callback);
  }
}