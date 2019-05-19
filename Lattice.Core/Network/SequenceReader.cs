using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace Lattice.Core.Network
{
    public ref struct SequenceReader2
    {
        public int CurrentSpanIdx { get; private set; }
        public ReadOnlySpan<byte> CurrentSpan { get; private set; }

        public ReadOnlySequence<byte> Sequence { get; private set; }

        public bool End { get; private set; }

        public SequencePosition Position => Sequence.GetPosition(_position, _startPosition);

        private ReadOnlySequenceSegment<byte> _currentSegment;
        private SequencePosition _startPosition;
        private SequencePosition _currentPosition;

        private long _position;

        public SequenceReader2(ReadOnlySequence<byte> sequence)
        {
            Sequence = sequence;
            CurrentSpan = sequence.First.Span;
            CurrentSpanIdx = 0;

            _position = 0;
            _currentSegment = sequence.Start.GetObject() as ReadOnlySequenceSegment<byte>;
            _startPosition = sequence.Start;
            _currentPosition = _startPosition;

            End = CurrentSpan.Length == 0;

            if (End && !sequence.IsSingleSegment)
            {
                End = false;
                GetNextSpan();
            }
        }

        private void GetNextSpan()
        {
            if (!Sequence.IsSingleSegment)
            {
                var next = _currentSegment.Next;
                if (next != null)
                {
                    _currentSegment = next;
                    CurrentSpan = _currentSegment.Memory.Span;
                    CurrentSpanIdx = 0;
                }
            }

            End = true;
        }

        private void GetNextSpan2()
        {
            ReadOnlyMemory<byte> memory;
            if (!Sequence.IsSingleSegment)
            {
                while(Sequence.TryGet(ref _currentPosition, out memory, true))
                {

                }

                var next = _currentSegment.Next;
                if (next != null)
                {
                    _currentSegment = next;
                    CurrentSpan = _currentSegment.Memory.Span;
                    CurrentSpanIdx = 0;
                }
            }

            End = true;
        }

        public bool TryRead(out byte value)
        {
            if (End)
            {
                value = default;
                return false;
            }

            value = CurrentSpan[CurrentSpanIdx];
            CurrentSpanIdx++;
            _position++;

            if (CurrentSpanIdx >= CurrentSpan.Length)
            {
                GetNextSpan();
            }

            return true;
        }

        public bool TryReadInt(out int value)
        {
            value = 0;
            TryRead(out byte b1);
            TryRead(out byte b2);
            TryRead(out byte b3);
            TryRead(out byte b4);
            value |= b1 + (b2 << 8) + (b3 << 16) + (b4 << 24);

            return true;
        }

    }
}
