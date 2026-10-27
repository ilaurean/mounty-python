import { Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Board } from '../../../core/models';

@Component({
  selector: 'app-board-details',
  standalone: false,
  templateUrl: './board-details.html'
})
export class BoardDetails {
  protected readonly board = inject<{ board: Board }>(MAT_DIALOG_DATA).board;
}
