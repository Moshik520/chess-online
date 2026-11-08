import { Component,inject,signal } from '@angular/core';
import { GameService } from '../../services/game';
import { Board } from '../../components/board/board';

@Component({
  imports: [Board],
  selector: 'app-lobby',
  styleUrl: './lobby.scss',
  templateUrl: './lobby.html',
})
export class Lobby {
  protected readonly game = inject(GameService);
  protected readonly joinCode = signal('');

  protected onPieceSelected(square: string | null) {
    if (square === null) {
      this.game.clearLegalMoves();
      return;
    }

    this.game.getLegalMoves(square);
  }
}
