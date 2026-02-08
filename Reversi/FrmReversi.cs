// FrmReversi.cs
// Defines the behavior of controls on frmReversi
// Programmed by Jonathan Feucht, 2015

using Reversi.Properties;

using System;
using System.Drawing;
using System.Windows.Forms;

namespace Reversi {
	public partial class FrmReversi : Form {
		private Point cursorLocation = new();

		private readonly GameEngine.BoardSquareTypes[,] boardData = new GameEngine.BoardSquareTypes[8, 8];
		private readonly BlendPictureBox[,] tiles = new BlendPictureBox[8, 8];

		public FrmReversi() {
			InitializeComponent();
		}

		private void CreateBoard() {
			DeleteBoard();

			Random rnd = new();
			for (Int32 y = 0; y < 8; y++) {
				for (Int32 x = 0; x < 8; x++) {
					boardData[x, y] = GameEngine.BoardSquareTypes.Blank;
					tiles[x, y] = new BlendPictureBox {
						RotationAngle = rnd.Next(0, 360),
						Size = new(GameContainer.Panel1.ClientSize.Width / 8, GameContainer.Panel1.ClientSize.Height / 8),
						Location = new(x * (GameContainer.Panel1.ClientSize.Width / 8), y * (GameContainer.Panel1.ClientSize.Height / 8)),
						SizeMode = PictureBoxSizeMode.StretchImage,
						BackColor = Color.Transparent,
						Image1 = Resources.whitechecker,
						Image2 = Resources.blackchecker,
						Blend = 1F,
						Visible = true,
						Empty = true
					};
					tiles[x, y].MouseMove += Tile_MouseMove;
					GameContainer.Panel1.Controls.Add(tiles[x, y]);

				}
			}
		}

		private void DisplayBoard() {
			Point thisLocation = new();

			for (thisLocation.Y = 0; thisLocation.Y < 8; thisLocation.Y++) {
				for (thisLocation.X = 0; thisLocation.X < 8; thisLocation.X++) {
					PlaceChip(GameEngine.GetSquare(thisLocation.X, thisLocation.Y), thisLocation);
				}
			}

			if (GameEngine.IsGameOver() == 0) {
				LblWinner.Visible = false;
				if (GameEngine.IsBlackTurn() == 0) {
					LblStatus.Text = "White's turn";
					PctCurColor.Image = Resources.whitechecker;
				} else {
					LblStatus.Text = "Black's turn";
					PctCurColor.Image = Resources.blackchecker;
				}
			} else {
				Int32 winner = GameEngine.Winner();
				PctCurColor.Image = null;
				LblStatus.Text = "Game over";
				LblWinner.Visible = true;
				if (winner > 0) {
					LblWinner.Text = "Black wins!";
				} else if (winner < 0) {
					LblWinner.Text = "White wins!";
				} else {
					LblWinner.Text = "Draw";
				}
			}

			LblProgress.Text = "Number of plays: " + GameEngine.GetNumPlays();

			while (TmMorph.Enabled) {
				Application.DoEvents();
			}
		}

		private void PlaceChip(GameEngine.BoardSquareTypes type, Point location) {
			if (boardData[location.X, location.Y] == GameEngine.BoardSquareTypes.Blank) {
				switch (type) {
					case GameEngine.BoardSquareTypes.Blank:
						tiles[location.X, location.Y].Empty = true;
						break;
					case GameEngine.BoardSquareTypes.Black:
						boardData[location.X, location.Y] = GameEngine.BoardSquareTypes.Black;
						tiles[location.X, location.Y].Blend = 1;
						tiles[location.X, location.Y].Visible = true;
						tiles[location.X, location.Y].Empty = false;
						break;
					case GameEngine.BoardSquareTypes.White:
						boardData[location.X, location.Y] = GameEngine.BoardSquareTypes.White;
						tiles[location.X, location.Y].Blend = 0;
						tiles[location.X, location.Y].Visible = true;
						tiles[location.X, location.Y].Empty = false;
						break;
				}
			} else {
				if (type == GameEngine.BoardSquareTypes.Blank) {
					boardData[location.X, location.Y] = GameEngine.BoardSquareTypes.Blank;
					tiles[location.X, location.Y].Image = null;
					tiles[location.X, location.Y].Empty = true;
				} else if (boardData[location.X, location.Y] != type) {
					// Begin the morphing process
					boardData[location.X, location.Y] = type;
					tiles[location.X, location.Y].Visible = true;
					tiles[location.X, location.Y].Empty = false;
					TmMorph.Enabled = true;
				}
			}
		}

		// Delete all picture boxes
		private void DeleteBoard() {
			for (Int32 y = 0; y < 8; y++) {
				for (Int32 x = 0; x < 8; x++) {
					_ = new Random();

					if (tiles[x, y] != null) {
						Controls.Remove(tiles[x, y]);
						tiles[x, y] = null;
					}
				}
			}
		}

		private void FrmReversi_Load(Object sender, EventArgs e) {
			GameEngine.NewGame();

			System.Diagnostics.Debug.Write(GameEngine.ToString());

			CreateBoard();
			DisplayBoard();
			ComputerPlay();
		}

		private void Tile_MouseMove(Object sender, MouseEventArgs e) {
			BlendPictureBox refObject = sender as BlendPictureBox;

			if (refObject.Empty) {
				if ((!IsComputerTurn()) && (TmMorph.Enabled == false)) {
					cursorLocation.X = 8 * (refObject.Left + (refObject.Width / 2)) / GameContainer.Panel1.ClientSize.Width;
					cursorLocation.Y = 8 * (refObject.Top + (refObject.Height / 2)) / GameContainer.Panel1.ClientSize.Height;

					if (GameEngine.IsValidMove(cursorLocation.X, cursorLocation.Y) != 0) {
						PctCursor.Location = refObject.Location;
						PctCursor.Size = refObject.Size;
						PctCursor.Visible = true;
					} else {
						PctCursor.Visible = false;
					}
				}
			} else {
				PctCursor.Visible = false;
			}
		}

		private void PctCursor_Click(Object sender, EventArgs e) {
			PctCursor.Visible = false;

			GameEngine.Play(cursorLocation.X, cursorLocation.Y);
			DisplayBoard();
			ComputerPlay();
		}

		private void ComputerPlay() {
			while (IsComputerTurn()) {
				LblStatus.Text += ": Waiting on computer...";
				Application.DoEvents();
				GameEngine.ComputerPlay();
				DisplayBoard();

				if (IsComputerTurn()) {
					// Give a second so the human can see the move
					System.Threading.Thread.Sleep(1000);
				}
			}
		}

		private Boolean IsComputerTurn() {
			if (GameEngine.IsGameOver() == 0) {
				if (GameEngine.IsBlackTurn() == 0) {
					if (ChkBlack.Checked || ChkNeither.Checked) {
						return true;
					}
				} else {
					if (ChkWhite.Checked || ChkNeither.Checked) {
						return true;
					}
				}
			}
			return false;
		}

		private void TmMorph_Tick(Object sender, EventArgs e) {
			Boolean done = true;

			for (Int32 y = 0; y < 8; y++) {
				for (Int32 x = 0; x < 8; x++) {
					GameEngine.BoardSquareTypes thisSquare = boardData[x, y];
					Single curBlend = tiles[x, y].Blend;

					if (thisSquare == GameEngine.BoardSquareTypes.White) {
						if (curBlend != 0F) {
							done = false;
							curBlend -= 0.05F;
							if (curBlend < 0F) {
								curBlend = 0F;
							}
							tiles[x, y].Blend = curBlend;
						}
					} else if (thisSquare == GameEngine.BoardSquareTypes.Black) {
						if (curBlend != 1F) {
							done = false;
							curBlend += 0.05F;
							if (curBlend > 1F) {
								curBlend = 1F;
							}
							tiles[x, y].Blend = curBlend;
						}
					}
				}
			}

			if (done) {
				TmMorph.Enabled = false;
			}
		}

		private void BtnExit_Click(Object sender, EventArgs e) {
			Application.Exit();
		}

		private void BtnNewGame_Click(Object sender, EventArgs e) {
			GameEngine.NewGame();
			DisplayBoard();
			ComputerPlay();
		}

		private void LblWinner_Click(Object sender, EventArgs e) {
			LblWinner.Visible = false;
		}

		private void ChkWhite_CheckedChanged(Object sender, EventArgs e) {
			ComputerPlay();
		}

		private void ChkBlack_CheckedChanged(Object sender, EventArgs e) {
			ComputerPlay();
		}

		private void ChkNeither_CheckedChanged(Object sender, EventArgs e) {
			ComputerPlay();
		}
	}
}