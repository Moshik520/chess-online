import { Component,inject,signal } from '@angular/core';
import { GameService } from '../../services/game';

@Component({
  imports: [],
  selector: 'app-lobby',
  styleUrl: './lobby.scss',
  templateUrl: './lobby.html',
})
export class Lobby {
  protected readonly game = inject(GameService);
  protected readonly joinCode = signal('');

}
