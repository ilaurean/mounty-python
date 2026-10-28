import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { applyEach, form, min, required } from '@angular/forms/signals';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Board, CreateOrderRequest, Order, UpdateOrderRequest } from '../../../core/models';
import { BoardsApi } from '../../inventory/boards.api';

export interface OrderFormData {
  order?: Order;
}

export interface OrderFormResult {
  create?: CreateOrderRequest;
  update?: UpdateOrderRequest;
}

interface ItemModel {
  boardID: string;
  quantity: number;
}

interface OrderModel {
  name: string;
  description: string;
  orderDate: string;
  items: ItemModel[];
}

@Component({
  selector: 'app-order-form',
  standalone: false,
  templateUrl: './order-form.html',
  styleUrl: './order-form.scss'
})
export class OrderForm implements OnInit {
  private readonly dialogRef = inject<MatDialogRef<OrderForm, OrderFormResult>>(MatDialogRef);
  private readonly data = inject<OrderFormData>(MAT_DIALOG_DATA);
  private readonly boardsApi = inject(BoardsApi);

  protected readonly isNew = !this.data.order;
  protected readonly canEditItems = this.isNew || this.data.order?.status === 'Created';
  protected readonly boards = signal<Board[]>([]);

  protected readonly model = signal<OrderModel>({
    name: this.data.order?.name ?? '',
    description: this.data.order?.description ?? '',
    orderDate: (this.data.order?.orderDate ?? new Date().toISOString()).substring(0, 10),
    items: (this.data.order?.items ?? []).map(i => ({ boardID: i.board.id, quantity: i.quantity }))
  });

  protected readonly orderForm = form(this.model, path => {
    required(path.name);
    required(path.orderDate);
    applyEach(path.items, item => {
      required(item.boardID);
      min(item.quantity, 1);
    });
  });

  protected readonly canSave = computed(
    () => this.orderForm().valid() && (!this.canEditItems || this.model().items.length > 0)
  );

  ngOnInit(): void {
    if (this.canEditItems) {
      this.boardsApi.search({ pageIndex: 0, pageSize: 100 }).subscribe(boards => this.boards.set(boards));
    }
  }

  protected addItem(): void {
    this.model.update(m => ({ ...m, items: [...m.items, { boardID: '', quantity: 1 }] }));
  }

  protected removeItem(index: number): void {
    this.model.update(m => ({ ...m, items: m.items.filter((_, i) => i !== index) }));
  }

  protected save(): void {
    if (!this.canSave()) {
      return;
    }
    const value = this.model();
    const items = this.canEditItems ? value.items.map(item => {
      const board = this.boards().find(b => b.id === item.boardID)!;
      return {
        boardID: board.id,
        boardName: board.name,
        quantity: item.quantity,
        components: board.components.map(c => ({ name: c.name, quantity: c.quantity }))
      };
    }) : [];
    if (!this.isNew) {
      this.dialogRef.close({
        update: {
          name: value.name,
          description: value.description || null,
          ...(this.canEditItems ? { items } : {})
        }
      });
      return;
    }
    this.dialogRef.close({
      create: {
        name: value.name,
        description: value.description || null,
        orderDate: new Date(value.orderDate).toISOString(),
        items
      }
    });
  }
}
