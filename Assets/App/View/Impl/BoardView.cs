﻿#pragma warning disable 649

namespace App.View.Impl
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;
    using UniRx;
    using Dekuple;
    using Dekuple.Agent;
    using Model;
    using Agent;
    using Common;

    /// <summary>
    /// View of the play board during a game.
    /// </summary>
    public class BoardView
        : GameViewBase<IBoardAgent>
        , IBoardView
    {
        #region Unity3d Properties

        public AudioClip CheckSound;
        public Material BlackPieceMaterial;
        public Material WhitePieceMaterial;
        public PieceView PieceViewPrefab;
        public SquareView BlackPrefab;
        public SquareView WhitePrefab;
        public Transform SquaresRoot;
        public Transform PiecesRoot;
        public int BoardWidth;
        public int BoardHeight;
        public BoardOverlayView OverlayView;
        #endregion

        public IReadOnlyReactiveCollection<IPieceView> Pieces => _pieces;
        public Material BlackMaterial => BlackPieceMaterial;
        public Material WhiteMaterial => WhitePieceMaterial;
        public IReadOnlyReactiveProperty<ISquareView> HoverSquare => _hoverSquare;
        public IReadOnlyReactiveProperty<IPieceView> HoverPiece => _hoverPiece;
        public IReadOnlyReactiveProperty<int> Width => Agent.Width;
        public IReadOnlyReactiveProperty<int> Height => Agent.Height;

        private int _squareBitMask;
        private List<SquareView> _squares;
        private readonly ReactiveProperty<ISquareView> _hoveredSquare = new ReactiveProperty<ISquareView>();
        private readonly ReactiveProperty<ISquareView> _hoverSquare = new ReactiveProperty<ISquareView>();
        private readonly ReactiveProperty<IPieceView> _hoverPiece = new ReactiveProperty<IPieceView>();
        private readonly ReactiveCollection<IPieceView> _pieces = new ReactiveCollection<IPieceView>();
        private Transform _squareTransform;

        public override bool IsValid
        {
            get
            {
                Verbose(10, "Test Valid BoardView");
                if (!base.IsValid)
                    return false;
                Assert.AreEqual(_pieces.Count, Agent.Pieces.Count);
                var n = 0;
                foreach (var p in _pieces)
                    Assert.AreSame(p.Agent, Agent.Pieces.ElementAt(n++));
                return true;
            }
        }

        public string Print()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"BoardView: {_pieces.Count} pieces:");
            foreach (var p in _pieces)
                sb.AppendLine($"\t{p.Coord.Value} => {p.Agent.Model}");
            return sb.ToString();
        }

        protected override bool Create()
        {
            if (!base.Create())
                return false;
            
            _squareBitMask = LayerMask.GetMask("BoardSquare");
            _hoveredSquare.DistinctUntilChanged().Subscribe(sq => _hoverSquare.Value = sq);
            HoverSquare.Subscribe(sq =>
            {
                OverlayView.Clear();
                if (sq == null) return;
                var p = Agent.At(sq.Coord);
                if (p == null) return;
                ShowSquares(sq.Coord);
            });

            HoverPiece.Subscribe(p => { if (p != null) Info($"Dragging {p} @{HoverSquare.Value}"); });
            
            return true;
        }

        //public override void SetAgent(IViewBase view, IBoardAgent agent)
        public override void SetAgent(IAgent agent)
        {
            Assert.IsNotNull(agent);
            base.SetAgent(agent);
            var board = agent as IBoardAgent;
            Assert.IsNotNull(board);
            Clear();
            CreateBoard();

            board.Pieces.ObserveAdd().Subscribe(PieceAdded);
            board.Pieces.ObserveRemove().Subscribe(PieceRemoved);
        }

        /// <summary>
        /// At this point, the piece can and will be added to the board.
        ///
        /// Determine the consequences of that.
        /// </summary>
        /// <param name="add">The piece added</param>
        private void PieceAdded(CollectionAddEvent<IPieceAgent> add)
        {
            var agent = add.Value;
            var view = ViewRegistry.FromPrefab<IPieceView>(PieceViewPrefab, agent);
            view.GameObject.transform.SetParent(PiecesRoot);
            _pieces.Add(view);
            // TODO Agent.Model.TestForCheck(agent.Model.Color);
        }

        private void PieceRemoved(CollectionRemoveEvent<IPieceAgent> add)
        {
            var p = _pieces.ElementAt(add.Index);
            Assert.IsNotNull(p);
            _pieces.RemoveAt(add.Index);
            p.Destroy();
            // TODO Agent.Model.TestForCheck(agent.Model.Color);
        }

        public void ShowSquares(ICardModel model, ISquareView sq)
        {
            Assert.IsNotNull(sq);
            Assert.IsNotNull(model);

            var board = Agent.Model;
            var movements = board.GetMovements(sq.Coord, model.PieceType);
            var attacks = board.GetAttacks(sq.Coord, model.PieceType);
            AddOverlays(movements.Coords, attacks.Coords);
            OverlayView.Add(movements.Interrupts.Select(p => p.Coord.Value), Color.yellow);
            OverlayView.Add(attacks.Interrupts.Select(p => p.Coord.Value), Color.magenta);
        }

        private void ShowSquares(Coord coord)
        {
            var agent = Agent.At(coord);
            var board = Agent.Model;
            var movements = board.GetMovements(agent.Model);
            var attacks = board.GetAttacks(agent.Model);
            AddOverlays(movements.Coords, attacks.Coords);
            OverlayView.Add(movements.Interrupts.Select(p => p.Coord.Value), Color.yellow);
            OverlayView.Add(attacks.Interrupts.Select(p => p.Coord.Value), Color.magenta);
        }

        private void AddOverlays(IList<Coord> moves, IList<Coord> attacks)
        {
            OverlayView.Clear();
            OverlayView.Add(attacks, Color.red);
            if (moves.SequenceEqual(attacks))
                return;
            OverlayView.Add(moves, Color.green);
        }

        public IPieceView Get(Coord coord)
        {
            return _pieces.FirstOrDefault(p => p.Coord.Value == coord);
        }

        [ContextMenu("Board-Clear")]
        public void Clear()
        {
            foreach (Transform tr in SquaresRoot.transform)
                Destroy(tr.gameObject);
            foreach (Transform tr in PiecesRoot.transform)
                Destroy(tr.gameObject);
        }

        [ContextMenu("Board-Create")]
        private void CreateBoard()
        {
            Clear();
            var length = BlackPrefab.Length;
            Assert.AreEqual(BlackPrefab.Length, WhitePrefab.Length);
            var width = Width.Value;
            var height = Height.Value;
            const float z = 0.0f;
            var origin = new Vector3(-length*(width/2.0f - 1/2.0f), -length*(height/2.0f - 1/2.0f), 0);
            var c = 1;
            _squares = new List<SquareView>(width * height);
            for (var ny = 0; ny < height; ++ny)
            {
                for (var nx = 0; nx < width; ++nx)
                {
                    var white = ((c + nx) % 2) == 1;
                    var prefab = white ? WhitePrefab : BlackPrefab;
                    var square = Instantiate(prefab);
                    Assert.IsNotNull(square.GetComponent<Collider>());
                    var pos = origin + new Vector3(nx * length, ny * length, z);
                    _squareTransform = square.transform;
                    _squareTransform.localPosition = Vector3.zero;
                    _squareTransform.SetParent(SquaresRoot.transform);
                    _squareTransform.position = pos;
                    square.Coord = new Coord(nx, ny);
                    square.Color = white ? EColor.White : EColor.Black;
                    _squares.Add(square);
                }

                ++c;
            }
        }

        private SquareView At(int x, int y)
        {
            Assert.IsTrue(x >= 0 && x < Width.Value);
            Assert.IsTrue(y >= 0 && x < Height.Value);
            return _squares[y * Width.Value + x];
        }

        public SquareView At(Coord c)
        {
            return At(c.x, c.y);
        }

        public IResponse Remove(IPieceView pieceView)
        {
            return Agent.Remove(pieceView.Agent);
        }

        public IResponse MovePiece(IPieceView pieceView, Coord coord)
        {
            return Agent.Move(pieceView.Agent, coord);
        }

        public ISquareView TestRayCast(Vector3 screen)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray.origin, ray.direction, out var hit, Mathf.Infinity, _squareBitMask))
            {
                var square = hit.transform.gameObject.GetComponent<SquareView>();
                if (square != null)
                    return _hoveredSquare.Value = square;
            }
            else
            {
                return _hoveredSquare.Value = null;
            }

            return null;
        }

        protected override void Step()
        {
            base.Step();
            TestRayCast(Input.mousePosition);
        }
    }
}
