using System;
using System.Collections.Generic;

namespace MonoGameParticleEngine.Core
{
    /// <summary>
    /// Fixed-size object pool for Particle structs.
    /// Eliminates GC pressure by reusing pre-allocated array slots.
    /// Supports up to <see cref="Capacity"/> live particles simultaneously.
    /// </summary>
    public class ParticlePool
    {
        private readonly Particle[] _particles;
        private readonly Queue<int> _freeIndices;

        public int Capacity   { get; }
        public int ActiveCount => Capacity - _freeIndices.Count;

        // Read-only span of the raw array for fast iteration by emitters/renderers
        public ReadOnlySpan<Particle> Particles => _particles;

        public ParticlePool(int capacity = 5000)
        {
            Capacity     = capacity;
            _particles   = new Particle[capacity];
            _freeIndices = new Queue<int>(capacity);

            for (int i = 0; i < capacity; i++)
            {
                _particles[i].Reset();
                _freeIndices.Enqueue(i);
            }
        }

        /// <summary>
        /// Allocates one particle from the free list.
        /// Returns the index into the internal array, or -1 if the pool is exhausted.
        /// </summary>
        public int Spawn()
        {
            if (_freeIndices.Count == 0) return -1;
            int idx = _freeIndices.Dequeue();
            _particles[idx].IsAlive = true;
            _particles[idx].Age     = 0f;
            return idx;
        }

        /// <summary>Returns particle at <paramref name="index"/> back to the free list.</summary>
        public void Kill(int index)
        {
            if (index < 0 || index >= Capacity) return;
            _particles[index].Reset();
            _freeIndices.Enqueue(index);
        }

        /// <summary>Direct read/write access used by the emitter during Update.</summary>
        public ref Particle GetRef(int index) => ref _particles[index];

        /// <summary>Kills all live particles and resets the pool to empty.</summary>
        public void Clear()
        {
            for (int i = 0; i < Capacity; i++)
            {
                if (_particles[i].IsAlive)
                    Kill(i);
            }
        }
    }
}
