import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { form } from '@angular/forms/signals';
import { MatDialog } from '@angular/material/dialog';
import { PageEvent } from '@angular/material/paginator';
import { Store } from '@ngrx/store';
import { filter as filterOperator } from 'rxjs';
import { AuthService } from '../../../core/auth.service';
import { ORDER_STATUSES, Order, OrderDetailsData, OrderSearch, OrderStatus } from '../../../core/models';
import { ConfirmDialog, ConfirmDialogData } from '../../../shared/confirm-dialog';
import { OrderDetails } from '../order-details/order-details';
import { OrderForm, OrderFormData, OrderFormResult } from '../order-form/order-form';
import { OrdersApi } from '../orders.api';
import { ordersActions, ordersFeature } from '../orders.store';

@Component({
  selector: 'app-order-list',
  standalone: false,
  templateUrl: './order-list.html'
})
export class OrderList implements OnInit {
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly api = inject(OrdersApi);
  protected readonly auth = inject(AuthService);

  protected readonly statuses = ORDER_STATUSES;
  protected readonly orders = this.store.selectSignal(ordersFeature.selectOrders);
  protected readonly loading = this.store.selectSignal(ordersFeature.selectLoading);
  protected readonly currentFilter = this.store.selectSignal(ordersFeature.selectFilter);

  protected readonly search = signal<OrderSearch>({
    name: this.currentFilter().name ?? '',
    state: this.currentFilter().state ?? ''
  });
  protected readonly searchForm = form(this.search);

  protected readonly pageSize = computed(() => this.currentFilter().pageSize ?? 20);
  protected readonly pageIndex = computed(() => this.currentFilter().pageIndex ?? 0);
  protected readonly total = computed(() => {
    const count = this.orders().length;
    return this.pageIndex() * this.pageSize() + count + (count === this.pageSize() ? 1 : 0);
  });

  protected readonly columns = computed(() => [
    'id',
    'name',
    ...(this.auth.isManager ? ['customerID'] : []),
    'orderDate',
    'items',
    'status',
    'actions'
  ]);

  ngOnInit(): void {
    this.load(this.pageIndex(), this.pageSize());
  }

  protected load(pageIndex: number, pageSize: number): void {
    const { name, state } = this.search();
    this.store.dispatch(
      ordersActions.load({ filter: { name, state: (state || undefined) as OrderStatus | undefined, pageIndex, pageSize } })
    );
  }

  protected onSearch(): void {
    this.load(0, this.pageSize());
  }

  protected onPage(event: PageEvent): void {
    this.load(event.pageIndex, event.pageSize);
  }

  protected canChange(order: Order): boolean {
    return order.status === 'Created';
  }

  protected details(order: Order): void {
    this.dialog.open<OrderDetails, OrderDetailsData>(OrderDetails, { data: { order }, width: '600px' });
  }

  protected create(): void {
    this.openForm({}).subscribe(result => {
      if (result.create) {
        this.store.dispatch(ordersActions.create({ request: result.create }));
      }
    });
  }

  protected edit(order: Order): void {
    this.openForm({ order }).subscribe(result => {
      if (result.update) {
        this.store.dispatch(ordersActions.update({ id: order.id, request: result.update }));
      }
    });
  }

  protected startProcessing(order: Order): void {
    this.store.dispatch(ordersActions.startProcessing({ id: order.id }));
  }

  protected complete(order: Order): void {
    this.store.dispatch(ordersActions.complete({ id: order.id }));
  }

  protected cancel(order: Order): void {
    this.dialog
      .open<ConfirmDialog, ConfirmDialogData, boolean>(ConfirmDialog, {
        data: { title: 'Cancel order', message: `Cancel order "${order.name}"?` }
      })
      .afterClosed()
      .pipe(filterOperator(confirmed => confirmed === true))
      .subscribe(() => this.store.dispatch(ordersActions.cancel({ id: order.id })));
  }

  protected download(order: Order): void {
    this.api.download(order.id).subscribe(response => {
      const disposition = response.headers.get('Content-Disposition') ?? '';
      const match = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disposition);
      const fileName = match ? decodeURIComponent(match[1]) : `order-${order.id}.json`;
      const url = URL.createObjectURL(response.body as Blob);
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = fileName;
      anchor.click();
      URL.revokeObjectURL(url);
    });
  }

  private openForm(data: OrderFormData) {
    return this.dialog
      .open<OrderForm, OrderFormData, OrderFormResult>(OrderForm, { data })
      .afterClosed()
      .pipe(filterOperator((result): result is OrderFormResult => !!result));
  }
}
