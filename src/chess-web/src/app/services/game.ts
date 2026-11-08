import { Service, signal, computed } from '@angular/core';
import * as signalR from '@microsoft/signalr';
export type GameState = 'idle' | 'waiting' | 'playing' | 'Checkmate' | 'Stalemate' | 'abandoned';

@Service()
export class GameService {
  readonly gameId = signal<string | null>(null);
  readonly fen = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly status = signal<GameState>('idle');
  readonly lastMove = signal<string | null>(null);
  readonly myColor = signal<'w' | 'b' | null>(null);
  readonly legalMoves = signal<string[]>([]);

  private readonly connection = new signalR.HubConnectionBuilder().withUrl('/hubs/game').build();

  private readonly connected = this.connection.start();

  readonly isMyTurn = computed(() => {
    const fen = this.fen();
    if (fen === null) {
      return false;
    }

    return fen.split(' ')[1] === this.myColor();
  });

  constructor() {
    this.connection.on('GameStarted', (fen: string) => {
      this.fen.set(fen);
      this.status.set('playing');
    });

    this.connection.on('MoveMade', (uci: string, fen: string) => {
      this.fen.set(fen);
      this.lastMove.set(uci);
      this.legalMoves.set([]);
    });

    this.connection.on('GameOver', (status: string) => {
      this.status.set(status as GameState);
    });

    this.connection.on('PlayerDisconnected', () => {
      this.status.set('abandoned');
      this.legalMoves.set([]);
    });
  }

  // Back to the lobby so the player can start or join another game.
  leaveGame() {
    this.gameId.set(null);
    this.fen.set(null);
    this.lastMove.set(null);
    this.myColor.set(null);
    this.legalMoves.set([]);
    this.error.set(null);
    this.status.set('idle');
  }

  async createGame() {
    this.error.set(null);
    try {
      await this.connected;
      const id = await this.connection.invoke<string>('CreateGame');
      this.myColor.set('w');
      this.gameId.set(id);
      this.status.set('waiting');
    } catch (e) {
      this.error.set(String(e));
    }
  }
  async joinGame(id: string) {
    this.error.set(null);
    try {
      await this.connected;
      await this.connection.invoke<string>('JoinGame', id);
      this.myColor.set('b');
      this.gameId.set(id);
      this.status.set('playing');
    } catch (e) {
      this.error.set(String(e));
    }
  }

  async makeMove(uci: string) {
    this.error.set(null);
    try {
      await this.connected;
      await this.connection.invoke<string>('MakeMove', this.gameId(), uci);
    } catch (e) {
      this.error.set(String(e));
    }
  }

  clearLegalMoves() {
    this.legalMoves.set([]);
  }

  async getLegalMoves(from: string) {
    this.error.set(null);
    try {
      await this.connected;
      const legalMoves = await this.connection.invoke<string[]>(
        'GetLegalMoves',
        this.gameId(),
        from,
      );
      this.legalMoves.set(legalMoves);
    } catch (e) {
      this.error.set(String(e));
    }
  }
}
