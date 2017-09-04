using System;
using System.Collections.Generic;

namespace Common
{
    internal class CommandLineArguments
    {
        public string DeQueue()
        {
            if (Count <= 0) throw usage;
            return cla.Dequeue();
        }

        public int DeQueueInt(string parameterName, int defaultValue)
        {
            if (Count <= 0) return defaultValue;
            return DequeInt(parameterName);
        }

        public int DequeInt(string parameterName)
        {
            int r;
            var s = DeQueue();
            if (!int.TryParse(s, out r))
                throw new Exception($"Ugyldig heltall '{s}' for parameter '{parameterName}'.");
            return r;
        }

        public TEnum DeQueueEnum<TEnum>(string parameterName) where TEnum : struct, IComparable
        {
            TEnum r;
            var s = DeQueue();
            if (Enum.TryParse(s, true, out r)) return r;
            try
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), s);
            }
            catch (Exception)
            {
                throw new Exception($"Ugyldig verdi '{s}' for parameter '{parameterName}'.");
            }            
        }

        public CommandLineArguments(IEnumerable<string> args, Exception usage)
        {
            this.usage = usage;
            foreach (var arg in args)
                cla.Enqueue(arg);
        }

        public int Count => cla.Count;

        private readonly Queue<string> cla = new Queue<string>();
        private readonly Exception usage;
    }
}