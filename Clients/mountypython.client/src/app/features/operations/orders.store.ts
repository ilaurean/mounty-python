import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { createActionGroup, createFeature, createReducer, emptyProps, on, props } from '@ngrx/store';
import { Observable, catchError, map, mergeMap, of, switchMap, tap } from 'rxjs';
import { loadSession, saveSession } from '../../core/session-storage';
import { CreateOrderRequest, Order, OrderAggregate, OrderFilter, OrderViewMode, SelectedOrder, UpdateOrderRequest } from '../../core/models';
import { OrdersApi } from './orders.api';

export interface OrdersState {
  orders: OrderAggregate[];
  loading: boolean;
  filter: OrderFilter;
  selected: SelectedOrder | null;
}

export const initialOrdersState: OrdersState = {
  orders: [],
  loading: false,
  selected: null,
  filter: loadSession<OrderFilter>('orders.filter', { pageIndex: 0, pageSize: 20 })
};

export const ordersActions = createActionGroup({
  source: 'Orders',
  events: {
    load: props<{ filter: OrderFilter }>(),
    'load success': props<{ orders: OrderAggregate[] }>(),
    open: props<{ id: string; mode: OrderViewMode }>(),
    'open success': props<{ selected: SelectedOrder }>(),
    close: emptyProps(),
    create: props<{ request: CreateOrderRequest }>(),
    update: props<{ id: string; request: UpdateOrderRequest }>(),
    'start processing': props<{ id: string }>(),
    complete: props<{ id: string }>(),
    cancel: props<{ id: string }>(),
    'order upserted': props<{ order: Order }>(),
    failure: emptyProps()
  }
});

export function toOrderAggregate(order: Order): OrderAggregate {
  return {
    pk: order.pk,
    id: order.id,
    name: order.name,
    customerID: order.customerID,
    orderDate: order.orderDate,
    status: order.status,
    boardsCount: order.items.reduce((total, item) => total + item.quantity, 0),
    componentsCount: order.items.reduce((total, item) => total + item.quantity * item.board.components.length, 0)
  };
}

export function upsertOrder(orders: OrderAggregate[], order: Order): OrderAggregate[] {
  const aggregate = toOrderAggregate(order);
  const exists = orders.some(o => o.id === aggregate.id);
  return exists ? orders.map(o => (o.id === aggregate.id ? aggregate : o)) : [aggregate, ...orders];
}

export const ordersFeature = createFeature({
  name: 'orders',
  reducer: createReducer(
    initialOrdersState,
    on(ordersActions.load, (state, { filter }) => ({ ...state, filter, loading: true })),
    on(ordersActions.loadSuccess, (state, { orders }) => ({ ...state, orders, loading: false })),
    on(ordersActions.orderUpserted, (state, { order }) => ({ ...state, orders: upsertOrder(state.orders, order) })),
    on(ordersActions.openSuccess, (state, { selected }) => ({ ...state, selected })),
    on(ordersActions.close, state => ({ ...state, selected: null })),
    on(ordersActions.failure, state => ({ ...state, loading: false }))
  )
});

@Injectable()
export class OrdersEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(OrdersApi);

  readonly load$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ordersActions.load),
      switchMap(({ filter }) =>
        this.api.search(filter).pipe(
          map(orders => ordersActions.loadSuccess({ orders })),
          catchError(() => of(ordersActions.failure()))
        )
      )
    )
  );

  readonly open$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ordersActions.open),
      switchMap(({ id, mode }) =>
        this.api.get(id).pipe(
          map(order => ordersActions.openSuccess({ selected: { order, mode } })),
          catchError(() => of(ordersActions.failure()))
        )
      )
    )
  );

  readonly persistFilter$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(ordersActions.load),
        tap(({ filter }) => saveSession('orders.filter', filter))
      ),
    { dispatch: false }
  );

  readonly create$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ordersActions.create),
      mergeMap(({ request }) => this.upsert(this.api.create(request)))
    )
  );

  readonly update$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ordersActions.update),
      mergeMap(({ id, request }) => this.upsert(this.api.update(id, request)))
    )
  );

  readonly startProcessing$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ordersActions.startProcessing),
      mergeMap(({ id }) => this.upsert(this.api.startProcessing(id)))
    )
  );

  readonly complete$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ordersActions.complete),
      mergeMap(({ id }) => this.upsert(this.api.complete(id)))
    )
  );

  readonly cancel$ = createEffect(() =>
    this.actions$.pipe(
      ofType(ordersActions.cancel),
      mergeMap(({ id }) => this.upsert(this.api.cancel(id)))
    )
  );

  private upsert(request: Observable<Order>) {
    return request.pipe(
      map(order => ordersActions.orderUpserted({ order })),
      catchError(() => of(ordersActions.failure()))
    );
  }
}
