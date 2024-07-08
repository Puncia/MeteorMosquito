using System.Diagnostics;

public class TimingsCircularBuffer<T>
{
    private T[] _buffer;
    private int _start;
    private int _end;
    private int _avg = -1;

    public TimingsCircularBuffer(int capacity)
    {
        _buffer = new T[capacity];
    }

    public void Push(T item)
    {
        _buffer[_end] = item;
        _end = (_end + 1) % _buffer.Length;
        if (_end == _start)
        {
            _start = (_start + 1) % _buffer.Length;
        }
    }

    public int Average()
    {
        int count = 0;
        int sum = 0;
        int i = _start;
        while (i != _end)
        {
            sum += Convert.ToInt32(_buffer[i]);
            count++;
            i = (i + 1) % _buffer.Length;
        }

        _avg = count > 0 ? sum / count : 0;

        return _avg;
    }
}
