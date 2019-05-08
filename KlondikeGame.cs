using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace klondike
{
    public partial class KlondikeGame : Form
    {
        private List<List<CardMapping>> Tables = new List<List<CardMapping>>();
        private List<CardMapping> cardMappingTables = new List<CardMapping>();

        private List<CardMapping> Stack = new List<CardMapping>();
        private List<CardMapping> Unused = new List<CardMapping>();

        private List<CardMapping> Hearts = new List<CardMapping>();
        private List<CardMapping> Diamonds = new List<CardMapping>();
        private List<CardMapping> Clubs = new List<CardMapping>();
        private List<CardMapping> Spades = new List<CardMapping>();

        // Сдвиг карты относительно другой при размещении
        private const int cardShift = 20;

        public KlondikeGame()
        {
            {
                InitializeComponent();

                cardMappingTables.Add(cardMappingTable1);
                cardMappingTables.Add(cardMappingTable2);
                cardMappingTables.Add(cardMappingTable3);
                cardMappingTables.Add(cardMappingTable4);
                cardMappingTables.Add(cardMappingTable5);
                cardMappingTables.Add(cardMappingTable6);
                cardMappingTables.Add(cardMappingTable7);

                for (int i = 0; i < 7; i++)
                {
                    cardMappingTables[i].columnNumber = i;
                }

            }
        }

        private void Klondike_Load(object sender, EventArgs e)
        {
            StartGame();
            DrawTables();
            DrawStack();
        }

        private void StartGame()
        {
            Deck deck = new Deck();
            deck.Shuffle();

            for (int i = 0; i < 7; i++)
            {
                List<CardMapping> cardCardMappingList = new List<CardMapping>();
                Tables.Add(cardCardMappingList);
            }

            for (int i = 0; i < 7; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    Card card = deck.Pop();
                    CardMapping cardMapping = new CardMapping(card.Type, CardPlacement.Table)
                    {
                        columnNumber = i
                    };

                    Tables[i].Add(cardMapping);
                }
            }

            for (int i = 0; i < 7; i++)
            {
                OpenCard(Tables[i].Last());
            }

            for (int i = 0; i < 24; i++)
            {
                Card card = deck.Pop();
                CardMapping cardMapping = new CardMapping(card.Type, CardPlacement.Stack);
                Stack.Add(cardMapping);
            }
        }

        private void DrawTables()
        {
            for (int i = 0; i < Tables.Count; i++)
            {
                for (int j = 0; j < Tables[i].Count; j++)
                {
                    LocateCard(Tables[i][j], cardMappingTables[i], j);
                }
            }
        }

        private void DrawStack()
        {
            for (int i = 0; i < 24; i++)
            {
                LocateCard(Stack[i], cardMappingStack);
            }
        }

        private void LocateCard(CardMapping sourceCard, CardMapping fixedCard, int multiplier = 0)
        {
            Controls.Add(sourceCard);
            sourceCard.Location = new Point(fixedCard.Location.X,
                fixedCard.Location.Y + multiplier * cardShift);
            sourceCard.Size = fixedCard.Size;
            sourceCard.SizeMode = PictureBoxSizeMode.StretchImage;
            sourceCard.BringToFront();
        }

        private void OpenCard(CardMapping cardMapping)
        {
            cardMapping.card.OpenCard();
            cardMapping.Image = (Bitmap)Properties.Resources.ResourceManager.GetObject(cardMapping.card.Type.ToString());
        }

        private void CloseCard(CardMapping cardMapping)
        {
            cardMapping.card.CloseCard();
            cardMapping.Image = Properties.Resources.Back;
        }

    }
}
