import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntil } from 'rxjs/operators';
import { Subject } from 'rxjs';

import { ToastrService } from 'ngx-toastr';
import { CheckoutService } from '../../shared/checkout.service';
import { SignalrService } from '../../shared/signalr.service';
import { HttpErrorResponse } from '@angular/common/http';

interface OrderDetails {
  items: number;
  total: number;
}

interface PaymentRequest {
  paymentType: number;
  amount: number;
}

type ServiceState = 'active' | 'halfOpen' | 'onBreak';

interface ServiceStateMessage {
  serviceName: string;
  state: ServiceState;
}

@Component({
  selector: 'app-checkout',
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.css'],
  standalone: true,
  imports: [CommonModule]
})
export class CheckoutComponent implements OnInit, OnDestroy {

  private destroy$ = new Subject<void>();

  paypalState: ServiceState | null = null;
  stripeState: ServiceState | null = null;
  selectedPaymentMethod: number | null = null;

  orderDetails: OrderDetails = { 
    items: 3, 
    total: 100.00 
  };

  isProcessing = false;
  errorMessage: string | null = null;

  constructor(
    private checkoutService: CheckoutService,
    private signalrService: SignalrService,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.initializeSignalRConnection();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeSignalRConnection(): void {
    try {
      this.signalrService.startConnection();
      
      this.signalrService.addServiceStateListener((message: any) => {
        if (message.serviceName === 'PayPal') {
          this.paypalState = message.state;
        } else if (message.serviceName === 'Stripe') {
          this.stripeState = message.state;
        }
      });  
    } catch (error) {
      console.error('SignalR initialization failed:', error);
      this.errorMessage = 'Payment services are currently unavailable';
    }
  }

  processPayment(paymentType: number): void {
    const serviceState = paymentType === 1 ? this.paypalState : this.stripeState;
    
    if (serviceState === 'onBreak') {
      this.errorMessage = `${paymentType} is currently unavailable`;
      return;
    }

    this.selectedPaymentMethod = paymentType;
    this.errorMessage = null;
  }

  completeCheckout(): void {
    if (!this.selectedPaymentMethod) {
      this.errorMessage = 'Please select a payment method';
      return;
    }

    if (this.isProcessing) return;

    this.isProcessing = true;
    this.errorMessage = null;

    const paymentRequest: PaymentRequest = {
      paymentType: this.selectedPaymentMethod,
      amount: this.orderDetails.total
    };

    this.checkoutService.processCheckout(paymentRequest)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.handleSuccessfulPayment(response);
        },
        error: (error:HttpErrorResponse) => {
          if(error.status == 200)
          {
            this.handleSuccessfulPayment(error);
          }
          else
          {
          this.handlePaymentError(error);
          }
        },
        complete: () => {
          this.isProcessing = false;
        }
      });
  }

  private handleSuccessfulPayment(response: any): void {
    this.toastr.success(`Payment of $${this.orderDetails.total} processed successfully via ${this.selectedPaymentMethod === 1  ? 'PayPal' : 'Stripe'}`);
    this.isProcessing = false;
    this.selectedPaymentMethod = null;
  }

  private handlePaymentError(error: any): void {

    const errorMessage = error.error?.message || 
                         error.message || 
                         `Payment failed with ${this.selectedPaymentMethod}`;
    
    this.errorMessage = errorMessage;
    this.isProcessing = false;
    this.selectedPaymentMethod = null;

    this.toastr.error(errorMessage);
  }
}