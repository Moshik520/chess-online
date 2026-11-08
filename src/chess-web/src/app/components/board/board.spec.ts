import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Board } from './board';

const STARTING_FEN = 'rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1';

describe('Board', () => {
  let fixture: ComponentFixture<Board>;
  let element: HTMLElement;

  async function render(inputs: Record<string, unknown>) {
    for (const [name, value] of Object.entries(inputs)) {
      fixture.componentRef.setInput(name, value);
    }
    await fixture.whenStable();
  }

  function squares(): HTMLElement[] {
    return Array.from(element.querySelectorAll<HTMLElement>('.square'));
  }

  // The squares are drawn a8..h1, so index = (7 - rank) * 8 + file.
  function squareAt(name: string, flipped = false): HTMLElement {
    const file = name.charCodeAt(0) - 97;
    const rank = Number(name[1]) - 1;
    const index = (7 - rank) * 8 + file;
    return squares()[flipped ? 63 - index : index];
  }

  function emittedMoves(): string[] {
    const moves: string[] = [];
    fixture.componentInstance.moveSelected.subscribe((move) => moves.push(move));
    return moves;
  }

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Board] }).compileComponents();
    fixture = TestBed.createComponent(Board);
    element = fixture.nativeElement;
  });

  it('draws 64 squares with the starting pieces', async () => {
    await render({ fen: STARTING_FEN });

    expect(squares()).toHaveLength(64);
    expect(squareAt('e1').querySelector('.piece.white')?.textContent).toBe('♚');
    expect(squareAt('d8').querySelector('.piece:not(.white)')?.textContent).toBe('♛');
    expect(squareAt('e4').querySelector('.piece')).toBeNull();
  });

  it('colours a1 dark and h1 light', async () => {
    await render({ fen: STARTING_FEN });

    expect(squareAt('a1').classList).toContain('dark');
    expect(squareAt('h1').classList).not.toContain('dark');
  });

  it('puts h1 in the top-left corner when flipped', async () => {
    await render({ fen: STARTING_FEN, flipped: true });

    expect(squares()[0].querySelector('.piece.white')?.textContent).toBe('♜');
    expect(squareAt('e1', true).querySelector('.piece')?.textContent).toBe('♚');
  });

  it('emits a move after clicking a piece and then a target', async () => {
    await render({ fen: STARTING_FEN });
    const moves = emittedMoves();

    squareAt('e2').click();
    await fixture.whenStable();
    expect(squareAt('e2').classList).toContain('selected');

    squareAt('e4').click();
    await fixture.whenStable();

    expect(moves).toEqual(['e2e4']);
    expect(squareAt('e2').classList).not.toContain('selected');
  });

  it('does not select an empty square', async () => {
    await render({ fen: STARTING_FEN });
    const moves = emittedMoves();

    squareAt('e4').click();
    squareAt('e5').click();
    await fixture.whenStable();

    expect(moves).toEqual([]);
  });

  it('switches the selection when clicking another own piece', async () => {
    await render({ fen: STARTING_FEN });
    const moves = emittedMoves();

    squareAt('e2').click();
    squareAt('d2').click();
    await fixture.whenStable();

    expect(moves).toEqual([]);
    expect(squareAt('d2').classList).toContain('selected');
  });

  it('asks for the promotion piece before sending the move', async () => {
    await render({ fen: '8/4P3/8/8/8/8/8/k6K w - - 0 1' });
    const moves = emittedMoves();

    squareAt('e7').click();
    squareAt('e8').click();
    await fixture.whenStable();
    expect(moves).toEqual([]);

    element.querySelector<HTMLButtonElement>('.choices button[aria-label="n"]')!.click();
    await fixture.whenStable();

    expect(moves).toEqual(['e7e8n']);
    expect(element.querySelector('.promotion')).toBeNull();
  });

  it('marks the legal targets and the last move', async () => {
    await render({ fen: STARTING_FEN, legalTargets: ['e3', 'e4'], lastMove: 'g1f3' });

    expect(squareAt('e3').classList).toContain('legal-target');
    expect(squareAt('e4').classList).toContain('legal-target');
    expect(squareAt('e5').classList).not.toContain('legal-target');
    expect(squareAt('g1').classList).toContain('last-move');
    expect(squareAt('f3').classList).toContain('last-move');
  });
});
