﻿using App.Common;
using App.View.Impl1;
using UniRx;
using UnityEngine;

namespace App.View
{
    using Agent;

    /// <summary>
    /// View of the Board in the scene.
    /// </summary>
    public interface IBoardView
        : IView<IBoardAgent>
        , IPrintable
    {
        IReadOnlyReactiveProperty<int> Width { get; }
        IReadOnlyReactiveProperty<int> Height { get; }
        IReadOnlyReactiveProperty<ISquareView> HoverSquare { get; }

        Material BlackMaterial { get; }
        Material WhiteMaterial { get; }

        IPieceView Get(Coord coord);
        IPieceView PlacePiece(ICardView view, Coord cood);
        //void ShowSquares(IPieceView pieceView);
        void Remove(IPieceView pieceView);
        void MovePiece(IPieceView pieceView, Coord coord);

        ISquareView TestRayCast(Vector3 screen);
        void ShowSquares(ICardView cardView, ISquareView sq);
    }
}
