import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { createActionGroup, createFeature, createReducer, emptyProps, on, props } from '@ngrx/store';
import { Observable, catchError, map, mergeMap, of, switchMap } from 'rxjs';
import { CreateOrderRequest, Order, OrderFilter, UpdateOrderRequest } from '../../core/models';
import { OrdersApi } from './orders.api';

export interface OrdersState {
  orders: Order[];
  loading: boolean;
  filter: OrderFilter;
}

export const initialOrdersState: OrdersState = {
  orders: [],
  loading: false,
  filter: { pageIndex: 0, pageSize: 20 }
};

export const ordersActions = createActionGroup({
  source: 'Orders',
  events: {
    load: props<{ filter: OrderFilter }>(),
    'load success': props<{ orders: Order[] }>(),
    create: props<{ request: CreateOrderRequest }>(),
    update: props<{ id: string; request: UpdateOrderRequest }>(),
    'start processing': props<{ id: string }>(),
    complete: props<{ id: string }>(),
    cancel: props<{ id: string }>(),
    'order upserted': props<{ order: Order }>(),
    failure: emptyProps()
  }
});

export function upsertOrder(orders: Order[], order: Order): Order[] {
  const exists = orders.some(o => o.id === order.id);
  return exists ? orders.map(o => (o.id === order.id ? order : o)) : [order, ...orders];
}

export const ordersFeature = createFeature({
  name: 'orders',
  reducer: createReducer(
    initialOrdersState,
    on(ordersActions.load, (state, { filter }) => ({ ...state, filter, loading: true })),
    on(ordersActions.loadSuccess, (state, { orders }) => ({ ...state, orders, loading: false })),
    on(ordersActions.orderUpserted, (state, { order }) => ({ ...state, orders: upsertOrder(state.orders, order) })),
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
