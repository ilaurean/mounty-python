import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AuthService } from '../../../core/auth.service';
import { OrderDetailsData } from '../../../core/models';

@Component({
  selector: 'app-order-details',
  standalone: false,
  templateUrl: './order-details.html'
})
export class OrderDetails {
  protected readonly order = inject<OrderDetailsData>(MAT_DIALOG_DATA).order;
  protected readonly auth = inject(AuthService);
}
