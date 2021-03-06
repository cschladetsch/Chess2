﻿// ReSharper disable UnusedMember.Local

namespace App.View.Impl
{
    using UnityEngine;
    using UnityEngine.UI;
    using UniRx;
    using CoLib;
    using Dekuple;
    using Dekuple.Agent;
    using Dekuple.View;
    using Common;

    /// <summary>
    /// An item that can be dragged with the mouse, such as a card or a piece.
    /// </summary>
    /// <typeparam name="TIAgent"></typeparam>
    public abstract class Draggable<TIAgent>
        : GameViewBase<TIAgent>
        where TIAgent : class, IAgent
    {
        public Image Image;
        public AudioClip ReturnToStartClip;
        public IReadOnlyReactiveProperty<IViewBase> MouseOver => _mouseOver;
        public IReadOnlyReactiveProperty<ISquareView> SquareOver => _squareOverFiltered;

        private bool _dragging;
        private Vector3 _offset;
        private Vector3 _screenPoint;
        private Vector3 _startLocation;
        private Ref<Color> _backgroundColor;
        private const float ScaleTime = 0.230f;
        private readonly Vector3 _cursorOffset = new Vector3(0, -0.15f, 0);
        private const double ImageAlphaAnimDuration = 0.5;
        private readonly ReactiveProperty<IViewBase> _mouseOver = new ReactiveProperty<IViewBase>();
        private readonly ReactiveProperty<ISquareView> _squareOver = new ReactiveProperty<ISquareView>();
        private readonly ReactiveProperty<ISquareView> _squareOverFiltered = new ReactiveProperty<ISquareView>();

        private float _lastPickTime;
        private float _minPickDifference = 0.3f;
        private float _oldZ;

        protected abstract bool MouseDown();
        protected abstract void MouseHover();
        protected abstract void MouseUp(IBoardView board, Coord coord);

        protected override bool Create()
        {
            if (!base.Create())
                return false;
            
            if (Image != null)
                _backgroundColor = new Ref<Color>(() => Image.color, c => Image.color = c);
            else
                _backgroundColor = new Ref<Color>(() => Color.cyan, c => { });

            _squareOver.DistinctUntilChanged().Subscribe(s => _squareOverFiltered.Value = s);

            return true;
        }

        private void OnMouseEnter()
        {
            if (Time.time - _lastPickTime < _minPickDifference)
                return;
            _lastPickTime = Time.time;
            if (ScaleTo(1.5f))
                _mouseOver.Value = this;
        }

        private void OnMouseExit()
        {
            if (ScaleTo(1.0f))
                _mouseOver.Value = null;
        }

        private bool ScaleTo(float scale)
        {
            if (_dragging)
                return false;
            _Queue.RunToEnd();
            var pos = GameObject.transform.position;
            if (scale > 1)
            {
                _oldZ = pos.z;
                pos.z = -10;
            }
            else
                pos.z = _oldZ;

            _Queue.Sequence(
                Cmd.ScaleTo(gameObject, scale, ScaleTime),
                Cmd.MoveTo(gameObject, pos, 0)
            );
            return true;
        }

        private void OnMouseOver()
        {
            MouseHover();
        }

        private void OnMouseDown()
        {
            _startLocation = transform.localPosition;
            if (!MouseDown())
                return;

            _dragging = true;
            _screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            var mp = Input.mousePosition;
            _offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(mp.x, mp.y, _screenPoint.z));

            _Queue.Sequence(
                Cmd.Parallel(
                    Cmd.ScaleTo(gameObject, 1, ScaleTime),
                    Cmd.AlphaTo(_backgroundColor, 0, ImageAlphaAnimDuration, Ease.Smooth())
                )
            );
        }

        private void OnMouseDrag()
        {
            if (!_dragging)
                return;

            var mp = Input.mousePosition;
            var cursorPoint = new Vector3(mp.x, mp.y, _screenPoint.z);
            var cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint);
            transform.position = cursorPosition + _cursorOffset;
            transform.SetZ(-0.5f);
            _squareOver.Value = BoardView.TestRayCast(Input.mousePosition);
        }

        private void OnMouseUp()
        {
            _dragging = false;

            _Queue.RunToEnd();
            if (SquareOver.Value == null)
            {
                ReturnToStart();
                return;
            }

            Assert.IsNotNull(Agent);
            var coord = SquareOver.Value.Coord;
            MouseUp(BoardView, coord);
        }

        protected void ReturnToStart()
        {
            _Queue.Sequence(
                Cmd.Parallel(
                    Cmd.Do(() => _AudioSource.PlayOneShot(ReturnToStartClip)),
                    Cmd.AlphaTo(_backgroundColor, 1, ImageAlphaAnimDuration, Ease.Smooth()),
                    Cmd.ScaleTo(transform, 1, ScaleTime),
                    Cmd.MoveTo(
                        transform,
                        _startLocation,
                        0.34,
                        Ease.Smooth(),
                        true
                    )
                ),
                Cmd.Do(() => _dragging = false)
            );
        }
    }
}
