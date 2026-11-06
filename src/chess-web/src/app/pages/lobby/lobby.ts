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

}
