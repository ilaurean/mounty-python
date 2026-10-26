import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { provideEffects } from '@ngrx/effects';
import { provideState } from '@ngrx/store';
import { SharedModule } from '../../shared/shared-module';
import { OrderForm } from './order-form';
import { OrderList } from './order-list';
import { OrdersEffects, ordersFeature } from './orders.store';

const routes: Routes = [{ path: '', component: OrderList }];

@NgModule({
  declarations: [OrderList, OrderForm],
  imports: [SharedModule, RouterModule.forChild(routes)],
  providers: [provideState(ordersFeature), provideEffects(OrdersEffects)]
})
export class OperationsModule { }
