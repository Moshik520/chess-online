import { Service, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
export type GameState = 'idle' | 'waiting' | 'playing' | 'Checkmate' | 'Stalemate';

@Service()
export class GameService {

  readonly gameId = signal<string | null>(null);
  readonly fen = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly status = signal<GameState>('idle');
  readonly lastMove = signal<string | null>(null);
 
  private readonly connection = new signalR.HubConnectionBuilder()
    .withUrl('/hubs/game')
    .build();

  private readonly connected = this.connection.start();

  constructor() {
    this.connection.on('GameStarted', (fen: string) => {
      this.fen.set(fen);
      this.status.set('playing');
    });

     this.connection.on('MoveMade', (uci: string, fen: string) => {
      this.fen.set(fen);
      this.lastMove.set(uci);
    });

    this.connection.on('GameOver', (status: string) => {
      this.status.set(status as GameState);
    });
  }

  async createGame() {
    try {
      await this.connected;
      const id = await this.connection.invoke<string>('CreateGame');
      this.gameId.set(id);
      this.status.set('waiting');
    } catch (e) {
      this.error.set(String(e));
    }
  }
   async joinGame(id: string) {
    try {
      await this.connected;
      await this.connection.invoke<string>('JoinGame',id);
      this.gameId.set(id);
      this.status.set('playing');
    } catch (e) {
      this.error.set(String(e));
    }
  }

    async makeMove(uci: string) {
    try {
      await this.connected;
      await this.connection.invoke<string>('MakeMove',this.gameId(),uci);
    } catch (e) {
      this.error.set(String(e));
    }
  }

  
}
