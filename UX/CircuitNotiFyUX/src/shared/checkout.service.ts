
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {
  private apiUrl = 'https://localhost:7003/api/Checkout';

  constructor(private http: HttpClient) {}

  processCheckout(paymentRequest: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/process`, paymentRequest);
  }
}