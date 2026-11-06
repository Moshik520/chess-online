import { Component, computed, input, signal, output } from '@angular/core';

interface SquareView {
  name: string;
  piece: string | null;
  dark: boolean;
  symbol: string | null;
  white: boolean;
}

// Solid glyphs for both colours; CSS paints the white pieces.
// ︎ keeps browsers from turning the pawn into an emoji.
const SYMBOLS: Record<string, string> = {
  k: '♚',
  q: '♛',
  r: '♜',
  b: '♝',
  n: '♞',
  p: '♟︎',
};

@Component({
  imports: [],
  selector: 'app-board',
  styleUrl: './board.scss',
  templateUrl: './board.html',
})
export class Board {
  readonly fen = input.required<string>();

  protected readonly selected = signal<string | null>(null);
  readonly moveSelected = output<string>();

  protected readonly squares = computed(() => {
    const placement = this.fen().split(' ')[0];
    const rows = placement.split('/');
    const result: SquareView[] = [];

    for (let i = 0; i < 8; i++) {
      const rank = 7 - i; // the first row in FEN is rank 8
      let file = 0;

      for (const c of rows[i]) {
        if (/\d/.test(c)) {
          for (let k = 0; k < Number(c); k++) {
            result.push(this.square(rank, file, null));
            file++;
          }
        } else {
          result.push(this.square(rank, file, c));
          file++;
        }
      }
    }

    return result;
  });

  private square(rank: number, file: number, piece: string | null): SquareView {
    return {
      name: String.fromCharCode(97 + file) + (rank + 1),
      piece,
      dark: (rank + file) % 2 === 0,
      symbol: piece ? SYMBOLS[piece.toLowerCase()] : null,
      white: piece !== null && piece === piece.toUpperCase(),
    };
  }

  protected onSquareClick(square: SquareView) {
    const from = this.selected();
    if (from === null) {
      if (square.piece) {
        this.selected.set(square.name);
      }
      return;
    }
    if (from === square.name) {
      this.selected.set(null);
    } else {
      this.moveSelected.emit(from + square.name);
      this.selected.set(null);
    }
  }
}
