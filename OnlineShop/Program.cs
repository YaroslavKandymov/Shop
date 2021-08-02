using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineShop
{
    class Program
    {
        static void Main(string[] args)
        {
            Good iPhone12 = new Good("IPhone 12");
            Good iPhone11 = new Good("IPhone 11");

            Warehouse warehouse = new Warehouse();

            Shop shop = new Shop(warehouse);

            warehouse.Deliver(iPhone12, 10);
            warehouse.Deliver(iPhone11, 1);

            warehouse.ShowAllGoods();

            Cart cart = shop.Cart();
            cart.Add(iPhone12, 4);
            cart.Add(iPhone11,1); 

            cart.ShowAllGoods();

            Console.WriteLine(cart.Order().Paylink); 
        }
    }

    public interface IReaderCell
    {
        IReadOnlyList<IReadOnlyCell> Cells { get; }
    }

    public abstract class Storage
    {
        protected List<Cell> _cells;

        public Storage()
        {
            _cells = new List<Cell>();
        }

        public void Deliver(Good good, int count)
        {
            Cell newCell = new Cell(good, count);

            Cell cell = _cells.FirstOrDefault(c => c.Good == good);

            if (cell == null)
                _cells.Add(newCell);
            else
                cell.Merge(newCell);
        }
    }

    public class Warehouse : Storage, IReaderCell
    {
        public IReadOnlyList<IReadOnlyCell> Cells => _cells;

        public void PickUp(Good good, int count)
        {
            Cell cell = _cells.FirstOrDefault(c => c.Good == good);

            if (cell == null)
                throw new ArgumentOutOfRangeException(nameof(cell));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            if (cell.Count < count)
                throw new ArgumentOutOfRangeException("Такого товара нет на складе");

            if (cell.Count == count)
                _cells.Remove(cell);
            else
                cell.Remove(count);
        }
    }

    public class Shop
    {
        private Warehouse _warehouse;

        public Shop(Warehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public Cart Cart()
        {
            return new Cart(this);
        }

        public void PickUpFromWarehouse(Good good, int count)
        {
            _warehouse.PickUp(good, count);
        }
    }

    public class Cart : Storage, IReaderCell
    {
        private readonly Shop _shop;

        public string Paylink { get; private set; } = "Строка";

        public IReadOnlyList<IReadOnlyCell> Cells => _cells;

        public Cart()
        {
        }

        public Cart(Shop shop)
        {
            _cells = new List<Cell>();
            _shop = shop;
        }

        public void Add(Good good, int count)
        {
            _shop.PickUpFromWarehouse(good, count);

            Deliver(good, count);
        }

        public Cart Order()
        {
            return new Cart();
        }
    }

    public class Display
    {
        private IReadOnlyList<IReadOnlyCell> _cells;

        public Display(IReaderCell readerCell)
        {
            _cells = readerCell.Cells;
        }

        public void Show()
        {
            foreach (var cell in _cells)
                cell.ShowInfo();
        }
    }

    public interface IReadOnlyCell
    {
        int Count { get; }

        Good Good { get; }

        void ShowInfo();
    }

    public class Cell : IReadOnlyCell
    {
        public int Count { get; private set; }

        public Good Good { get; private set; }

        public Cell(Good good, int count)
        {
            Good = good;
            Count = count;
        }

        public void Merge(Cell newCell)
        {
            if (newCell.Good != Good)
                throw new InvalidOperationException();

            Count += newCell.Count;
        }

        public void ShowInfo()
        {
            Console.WriteLine($"Товар {Good.Name} + в количестве {Count} штук");
        }

        public void Remove(int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException();

            if (count <= Count)
                Count -= count;
            else
                throw new ArgumentOutOfRangeException("На складе недостаточно товара");
        }
    }

    public class Good
    {
        public string Name { get; private set; }

        public Good(string name)
        {
            Name = name;
        }
    }
}
