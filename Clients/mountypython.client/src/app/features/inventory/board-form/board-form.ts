import { Component, computed, inject, signal } from '@angular/core';
import { applyEach, disabled, form, min, required } from '@angular/forms/signals';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Board, ComponentRequest, CreateBoardRequest } from '../../../core/models';

export interface BoardFormData {
  board?: Board;
}

interface ComponentModel {
  pk?: string;
  name: string;
  description: string;
  quantity: number;
}

interface BoardModel {
  id: string;
  name: string;
  description: string;
  length: number | null;
  width: number | null;
  components: ComponentModel[];
}

@Component({
  selector: 'app-board-form',
  standalone: false,
  templateUrl: './board-form.html',
  styleUrl: './board-form.scss'
})
export class BoardForm {
  private readonly dialogRef = inject<MatDialogRef<BoardForm, CreateBoardRequest>>(MatDialogRef);
  private readonly data = inject<BoardFormData>(MAT_DIALOG_DATA);

  protected readonly isNew = !this.data.board;

  protected readonly model = signal<BoardModel>({
    id: this.data.board?.id ?? '',
    name: this.data.board?.name ?? '',
    description: this.data.board?.description ?? '',
    length: this.data.board?.length ?? null,
    width: this.data.board?.width ?? null,
    components: (this.data.board?.components ?? []).map(c => ({
      pk: c.pk,
      name: c.name,
      description: c.description ?? '',
      quantity: c.quantity
    }))
  });

  protected readonly boardForm = form(this.model, path => {
    required(path.id);
    required(path.name);
    disabled(path.id, () => !this.isNew);
    applyEach(path.components, component => {
      required(component.name);
      min(component.quantity, 1);
    });
  });

  protected readonly canSave = computed(() => this.boardForm().valid());

  protected addComponent(): void {
    this.model.update(m => ({ ...m, components: [...m.components, { name: '', description: '', quantity: 1 }] }));
  }

  protected removeComponent(index: number): void {
    this.model.update(m => ({ ...m, components: m.components.filter((_, i) => i !== index) }));
  }

  protected save(): void {
    if (!this.canSave()) {
      return;
    }
    const value = this.model();
    const components: ComponentRequest[] = value.components.map(c => ({
      pk: c.pk,
      name: c.name,
      description: c.description || null,
      quantity: c.quantity
    }));
    this.dialogRef.close({
      id: value.id,
      name: value.name,
      description: value.description || null,
      length: value.length,
      width: value.width,
      components
    });
  }
}
