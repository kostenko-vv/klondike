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

                AttachDragEventsToCardMapping(cardMappingHearts, false);
                AttachDragEventsToCardMapping(cardMappingDiamonds, false);
                AttachDragEventsToCardMapping(cardMappingClubs, false);
                AttachDragEventsToCardMapping(cardMappingSpades, false);
                AttachDragEventsToCardMapping(cardMappingUnused, false);
                AttachDragEventsToCardMapping(cardMappingStack);

                for (int i = 0; i < cardMappingTables.Count; i++)
                {
                    AttachDragEventsToCardMapping(cardMappingTables[i], false);
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
                    AttachDragEventsToCardMapping(cardMapping);
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
                AttachDragEventsToCardMapping(cardMapping);
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

        private void FinishGame()
        {
            int fullStackDropCount = 13;
            if (Hearts.Count != fullStackDropCount)
            {
                return;
            }
            if (Diamonds.Count != fullStackDropCount)
            {
                return;
            }
            if (Clubs.Count != fullStackDropCount)
            {
                return;
            }
            if (Spades.Count != fullStackDropCount)
            {
                return;
            }
            MessageBox.Show("YOU WIN!");
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

        private void AttachDragEventsToCardMapping(CardMapping cardMapping, bool withMouseEvents = true)
        {
            cardMapping.AllowDrop = true;
            cardMapping.DragEnter += new DragEventHandler(CardMapping_DragEnter);
            cardMapping.DragDrop += new DragEventHandler(CardMapping_DragDrop);
            if (withMouseEvents)
            {
                cardMapping.MouseDown += new MouseEventHandler(CardMapping_MouseDown);
            }
        }

        #region Обработчики событий по перемещению карт

        private void CardMapping_MouseDown(object sender, MouseEventArgs e)
        {
            CardMapping cardM = ((CardMapping)sender);

            // Костыль для возможности протмотра стека заново
            if (cardM.Tag != null)
            {
                MoveCardsFromUnusedToStack();
                return;
            }

            switch (cardM.card.Placement)
            {
                case CardPlacement.Stack:
                    MoveCardFromStackToUnused();
                    break;
                default:
                    if (cardM.card.isOpened())
                    {
                        cardM.DoDragDrop(cardM, DragDropEffects.Move);
                    }
                    break;
            }
        }

        private void CardMapping_DragDrop(object sender, DragEventArgs e)
        {
            CardMapping destinationCard = (CardMapping)sender;
            CardMapping sourceCard = (CardMapping)e.Data.GetData(typeof(CardMapping));

            switch (sourceCard.card.Placement)
            {
                case CardPlacement.Stack:
                    break;

                case CardPlacement.Drop:
                    if (destinationCard.Tag != null)
                    {
                        if (destinationCard.Tag.ToString() == "Table" && sourceCard.card.Rank == CardRank.King)
                        {
                            MoveCardsFromDropToTable(sourceCard, destinationCard);
                        }
                    }
                    else
                    {
                        switch (destinationCard.card.Placement)
                        {
                            case CardPlacement.Drop:
                                break;

                            case CardPlacement.Stack:
                                break;

                            case CardPlacement.Table:
                                if (destinationCard.card.Type != Tables[destinationCard.columnNumber].Last().card.Type)
                                {
                                    return;
                                }
                                if (sourceCard.card.PutField(destinationCard.card))
                                {
                                    MoveCardsFromDropToTable(sourceCard, destinationCard);
                                }
                                break;

                            case CardPlacement.Unused:
                                break;
                        }
                    }
                    break;

                case CardPlacement.Table:
                    // Empty
                    if (destinationCard.Tag != null)
                    {
                        if (destinationCard.Tag.ToString() == "Table" && sourceCard.card.Rank == CardRank.King)
                        {
                            MoveCardsFromTableToTable(sourceCard, destinationCard, true);
                        }
                        if (destinationCard.Tag.ToString() == "Drop" && sourceCard.card.Rank == CardRank.Ace)
                        {
                            MoveCardFromTableToDrop(sourceCard);
                            FinishGame();
                        }
                    }
                    else
                    {
                        switch (destinationCard.card.Placement)
                        {
                            case CardPlacement.Drop:
                                if (sourceCard.card.Type != Tables[sourceCard.columnNumber].Last().card.Type)
                                {
                                    return;
                                }
                                if (sourceCard.card.PutDrop(destinationCard.card))
                                {
                                    FinishGame();
                                    MoveCardFromTableToDrop(sourceCard);
                                }
                                break;
                            case CardPlacement.Stack:
                                break;
                            case CardPlacement.Table:
                                if (destinationCard.card.Type != Tables[destinationCard.columnNumber].Last().card.Type)
                                {
                                    return;
                                }
                                if (sourceCard.card.PutField(destinationCard.card))
                                {
                                    MoveCardsFromTableToTable(sourceCard, destinationCard);
                                }
                                break;
                            case CardPlacement.Unused:
                                break;
                        }
                    }
                    break;

                case CardPlacement.Unused:
                    // Empty
                    if (destinationCard.Tag != null)
                    {
                        if (destinationCard.Tag.ToString() == "Table" && sourceCard.card.Rank == CardRank.King)
                        {
                            MoveCardFromUnusedToTable(destinationCard.columnNumber, true);
                        }
                        if (destinationCard.Tag.ToString() == "Drop" && sourceCard.card.Rank == CardRank.Ace)
                        {
                            MoveCardFromUnusedToDrop(sourceCard);
                        }
                    }
                    else
                    {
                        switch (destinationCard.card.Placement)
                        {
                            case CardPlacement.Stack:
                                break;

                            case CardPlacement.Drop:
                                if (sourceCard.card.PutDrop(destinationCard.card))
                                {
                                    MoveCardFromUnusedToDrop(sourceCard);
                                }
                                break;

                            case CardPlacement.Table:
                                if (destinationCard.card.Type != Tables[destinationCard.columnNumber].Last().card.Type)
                                {
                                    return;
                                }
                                if (sourceCard.card.PutField(destinationCard.card))
                                {
                                    MoveCardFromUnusedToTable(destinationCard.columnNumber);
                                }
                                break;

                            case CardPlacement.Unused:
                                break;
                        }
                    }
                    break;
            }
        }

        private void CardMapping_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(CardMapping)) != null)
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        #endregion

        #region Обработчики перемещения карт по сцене

        private void MoveCardsFromTableToTable(CardMapping cardFrom, CardMapping cardTo, bool destinationIsEmpty = false)
        {
            int i = Tables[cardFrom.columnNumber].Count - 1;
            for (; i >= 0; i--)
            {
                if (Tables[cardFrom.columnNumber][i].card.Type == cardFrom.card.Type)
                {
                    break;
                }
            }

            int oldColumnNumber = cardFrom.columnNumber;
            if (destinationIsEmpty)
            {
                CardMapping cardM = Shift(Tables[oldColumnNumber], cardTo.columnNumber, i);
                LocateCard(cardM, cardMappingTables[cardTo.columnNumber]);
                cardM.columnNumber = cardTo.columnNumber;
            }

            while (i < Tables[oldColumnNumber].Count)
            {
                CardMapping cardM = Shift(Tables[oldColumnNumber], Tables[cardTo.columnNumber], true, i);
                LocateCard(cardM, cardMappingTables[cardTo.columnNumber], Tables[cardTo.columnNumber].Count - 1);
                cardM.columnNumber = cardTo.columnNumber;
            }

            if (Tables[oldColumnNumber].Count != 0)
            {
                OpenCard(Tables[oldColumnNumber].Last());
            }
        }
        private void MoveCardFromTableToDrop(CardMapping sourceCard)
        {
            CardMapping CardM;
            switch (sourceCard.card.Suite)
            {
                case CardSuite.Hearts:
                    CardM = Shift(Tables[sourceCard.columnNumber], Hearts, true);
                    LocateCard(CardM, cardMappingHearts);
                    break;
                case CardSuite.Diamonds:
                    CardM = Shift(Tables[sourceCard.columnNumber], Diamonds, true);
                    LocateCard(CardM, cardMappingDiamonds);
                    break;
                case CardSuite.Clubs:
                    CardM = Shift(Tables[sourceCard.columnNumber], Clubs, true);
                    LocateCard(CardM, cardMappingClubs);
                    break;
                case CardSuite.Spades:
                    CardM = Shift(Tables[sourceCard.columnNumber], Spades, true);
                    LocateCard(CardM, cardMappingSpades);
                    break;
                default:
                    CardM = new CardMapping();
                    break;
            }
            CardM.card.Placement = CardPlacement.Drop;

            if (Tables[CardM.columnNumber].Count != 0)
            {
                OpenCard(Tables[CardM.columnNumber].Last());
            }

            CardM.columnNumber = -1;
        }

        private void MoveCardFromStackToUnused()
        {
            CardMapping cardM = Shift(Stack, Unused, true);
            cardM.card.Placement = CardPlacement.Unused;
            LocateCard(cardM, cardMappingUnused);
        }

        private void MoveCardsFromUnusedToStack()
        {
            while (Unused.Count != 0)
            {
                CardMapping CardM = Shift(Unused, Stack, false);
                CardM.card.Placement = CardPlacement.Stack;
                LocateCard(CardM, cardMappingStack);
            }
        }
        private void MoveCardFromUnusedToTable(int columnNumber, bool destinationIsEmpty = false)
        {
            CardMapping cardM;
            if (destinationIsEmpty)
            {
                cardM = Shift(Unused, columnNumber, Unused.Count - 1);
            }
            else
            {
                cardM = Shift(Unused, Tables[columnNumber], true);
            }
            cardM.card.Placement = CardPlacement.Table;
            cardM.columnNumber = columnNumber;
            LocateCard(cardM, cardMappingTables[columnNumber], Tables[columnNumber].Count - 1);
        }
        private void MoveCardFromUnusedToDrop(CardMapping sourceCard)
        {
            CardMapping CardM;
            switch (sourceCard.card.Suite)
            {
                case CardSuite.Hearts:
                    CardM = Shift(Unused, Hearts, true);
                    LocateCard(CardM, cardMappingHearts);
                    break;
                case CardSuite.Diamonds:
                    CardM = Shift(Unused, Diamonds, true);
                    LocateCard(CardM, cardMappingDiamonds);
                    break;
                case CardSuite.Clubs:
                    CardM = Shift(Unused, Clubs, true);
                    LocateCard(CardM, cardMappingClubs);
                    break;
                case CardSuite.Spades:
                    CardM = Shift(Unused, Spades, true);
                    LocateCard(CardM, cardMappingSpades);
                    break;
                default:
                    CardM = new CardMapping();
                    break;
            }
            CardM.card.Placement = CardPlacement.Drop;
            CardM.columnNumber = -1;
        }

        private void MoveCardsFromDropToTable(CardMapping sourceCard, CardMapping destinationCard)
        {
            CardMapping cardM;
            switch (sourceCard.card.Suite)
            {
                case CardSuite.Hearts:
                    cardM = Shift(Hearts, Tables[destinationCard.columnNumber], true);
                    LocateCard(cardM, cardMappingTables[destinationCard.columnNumber], Tables[destinationCard.columnNumber].Count - 1);
                    break;
                case CardSuite.Diamonds:
                    cardM = Shift(Diamonds, Tables[destinationCard.columnNumber], true);
                    LocateCard(cardM, cardMappingTables[destinationCard.columnNumber], Tables[destinationCard.columnNumber].Count - 1);
                    break;
                case CardSuite.Clubs:
                    cardM = Shift(Clubs, Tables[destinationCard.columnNumber], true);
                    LocateCard(cardM, cardMappingTables[destinationCard.columnNumber], Tables[destinationCard.columnNumber].Count - 1);
                    break;
                case CardSuite.Spades:
                    cardM = Shift(Spades, Tables[destinationCard.columnNumber], true);
                    LocateCard(cardM, cardMappingTables[destinationCard.columnNumber], Tables[destinationCard.columnNumber].Count - 1);
                    break;
                default:
                    cardM = new CardMapping();
                    break;
            }
            cardM.card.Placement = CardPlacement.Table;
            cardM.columnNumber = destinationCard.columnNumber;
        }

        #endregion

        #region Обработчики сдвига карт

        private CardMapping Shift(List<CardMapping> From, List<CardMapping> To, bool OpenOrClose, int IndexFirstReplacedCard)
        {
            CardMapping cardM = From[IndexFirstReplacedCard];
            cardM.columnNumber = To.Last().columnNumber;
            From.Remove(From[IndexFirstReplacedCard]);
            if (OpenOrClose)
            {
                OpenCard(cardM);
            }
            else
            {
                CloseCard(cardM);
            }
            To.Add(cardM);
            return cardM;
        }

        private CardMapping Shift(List<CardMapping> From, int columnNumber, int IndexFirstReplacedCard)
        {
            CardMapping cardM = From[IndexFirstReplacedCard];
            cardM.columnNumber = columnNumber;
            From.Remove(From[IndexFirstReplacedCard]);
            OpenCard(cardM);
            Tables[columnNumber].Add(cardM);
            return cardM;
        }

        private CardMapping Shift(List<CardMapping> From, List<CardMapping> To, bool OpenOrClose)
        {
            CardMapping cardM = From.Last();
            From.Remove(From.Last());
            if (OpenOrClose)
            {
                OpenCard(cardM);
            }
            else
            {
                CloseCard(cardM);
            }

            To.Add(cardM);
            return cardM;
        }

        #endregion

    }
}
