import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { Lobby } from './lobby';
import { GameService, GameState } from '../../services/game';

// Stands in for GameService so the tests never open a real SignalR connection.
function createFakeGame() {
  return {
    gameId: signal<string | null>(null),
    fen: signal<string | null>(null),
    error: signal<string | null>(null),
    status: signal<GameState>('idle'),
    lastMove: signal<string | null>(null),
    myColor: signal<'w' | 'b' | null>(null),
    legalMoves: signal<string[]>([]),
    isMyTurn: signal(false),
    createGame: vi.fn(),
    joinGame: vi.fn(),
    makeMove: vi.fn(),
    getLegalMoves: vi.fn(),
    clearLegalMoves: vi.fn(),
    leaveGame: vi.fn(),
  };
}

describe('Lobby', () => {
  let fixture: ComponentFixture<Lobby>;
  let element: HTMLElement;
  let game: ReturnType<typeof createFakeGame>;

  beforeEach(async () => {
    game = createFakeGame();
    await TestBed.configureTestingModule({
      imports: [Lobby],
      providers: [{ provide: GameService, useValue: game }],
    }).compileComponents();

    fixture = TestBed.createComponent(Lobby);
    element = fixture.nativeElement;
    await fixture.whenStable();
  });

  function button(text: string): HTMLButtonElement {
    const found = Array.from(element.querySelectorAll('button')).find(
      (b) => b.textContent?.trim() === text,
    );
    if (!found) {
      throw new Error(`No button "${text}"`);
    }
    return found;
  }

  it('creates a game from the lobby', () => {
    button('New game').click();

    expect(game.createGame).toHaveBeenCalled();
  });

  it('joins with the typed code once one is entered', async () => {
    expect(button('Join').disabled).toBe(true);

    const input = element.querySelector('input')!;
    input.value = ' a7f3c2e1 ';
    input.dispatchEvent(new Event('input'));
    await fixture.whenStable();
    button('Join').click();

    expect(game.joinGame).toHaveBeenCalledWith('a7f3c2e1');
  });

  it('shows the code to share while waiting', async () => {
    game.status.set('waiting');
    game.gameId.set('a7f3c2e1');
    await fixture.whenStable();

    expect(element.querySelector('.game-code')?.textContent).toBe('a7f3c2e1');
  });

  it('shows whose turn it is during a game', async () => {
    game.fen.set('rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1');
    game.status.set('playing');
    game.isMyTurn.set(true);
    await fixture.whenStable();

    expect(element.querySelector('.status')?.textContent?.trim()).toBe('Your turn');
    expect(element.querySelector('.status')?.classList).toContain('my-turn');
  });

  it('tells the player when the opponent leaves and offers a way back', async () => {
    game.fen.set('rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1');
    game.status.set('abandoned');
    await fixture.whenStable();

    expect(element.querySelector('.status')?.textContent?.trim()).toBe(
      'Your opponent left the game',
    );
    button('Back to lobby').click();
    expect(game.leaveGame).toHaveBeenCalled();
  });

  it('shows server errors', async () => {
    game.error.set('Game not found');
    await fixture.whenStable();

    expect(element.querySelector('[role="alert"]')?.textContent).toContain('Game not found');
  });
});
