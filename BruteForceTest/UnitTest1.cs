using Microsoft.VisualStudio.TestTools.UnitTesting;
using Brute_Force.Model;
using Brute_Force.Persistence;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BruteForceTest
{
    [TestClass]
    public class UnitTest1
    {
        private ModelClass _model;
        private Depot _mockedTable;
        private Mock<IDataAccess> _mock;


        [TestInitialize]
        public void Initialize()
        {
            _mockedTable = new Depot(5,5);
            _mockedTable[0, 0] = 'D';
            _mockedTable[0, 1] = '0';
            _mockedTable[0, 2] = '0';
            _mockedTable[0, 3] = '0';
            _mockedTable[0, 4] = 'D';

            _mockedTable[1, 0] = '0';
            _mockedTable[1, 1] = '0';
            _mockedTable[1, 2] = 'P';
            _mockedTable.Products[1, 2].Add(1);
            _mockedTable[1, 3] = '0';
            _mockedTable[1, 4] = '0';
            
            _mockedTable[2, 0] = 'R';
            _mockedTable[2, 1] = '0';
            _mockedTable[2, 2] = 'P';
            _mockedTable.Products[2, 2].Add(2);
            _mockedTable[2, 3] = '0';
            _mockedTable[2, 4] = 'R';

            _mockedTable[3, 0] = '0';
            _mockedTable[3, 1] = '0';
            _mockedTable[3, 2] = 'P';
            _mockedTable.Products[3, 2].Add(1);
            _mockedTable[3, 3] = '0';
            _mockedTable[3, 4] = '0';

            _mockedTable[4, 0] = '0';
            _mockedTable[4, 1] = 'S';
            _mockedTable[4, 2] = '0';
            _mockedTable[4, 3] = 'S';
            _mockedTable[4, 4] = '0';

            _mock = new Mock<IDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(_mockedTable));// a mock a LoadAsync mûveletben bármilyen paraméterre az elõre beállított játéktáblát fogja visszaadni


            _model = new ModelClass(5,5, _mock.Object);
            _model.EndOfSimulation += new EventHandler<EventArgs>(Model_End);
        }


        [TestMethod]
        public void Initializerfor5x5()
        {
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            Assert.AreEqual(_model.Depo[0, 0], 'D');
            Assert.AreEqual(_model.Depo[0, 4], 'D');
            Assert.AreEqual(1, _model.Depo.Products[1, 2].Count);
            Assert.AreEqual(1, _model.Depo.Products[2, 2].Count);
            Assert.AreEqual(1, _model.Depo.Products[3, 2].Count);
            Assert.AreEqual(2, _model.RobotsListProp.Count);
        }

        [TestMethod]
        public void RobotRoutePlanIn5x5()
        {
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.MoveAtTick();
            Assert.AreNotEqual(0, _model.RobotsListProp[0].Route.Count);
            Assert.AreNotEqual(0, _model.RobotsListProp[1].Route.Count);
        }

        [TestMethod]
        public void RobotRotate()
        {
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.MoveAtTick();
            Direction first = _model.RobotsListProp[0].Direction;
            Direction second = _model.RobotsListProp[1].Direction;
            Assert.AreNotEqual(0, _model.RobotsListProp[0].Route.Count);
            Assert.AreNotEqual(0, _model.RobotsListProp[1].Route.Count);
            _model.MoveAtTick();
            Assert.AreNotEqual(first, _model.RobotsListProp[0].Position);
            Assert.AreNotEqual(second, _model.RobotsListProp[1].Position);
        }

        [TestMethod]
        public void RobotMove()
        {
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.MoveAtTick();
            Coordinate first = _model.RobotsListProp[0].Position;
            Coordinate second = _model.RobotsListProp[1].Position;
            Assert.AreNotEqual(0, _model.RobotsListProp[0].Route.Count);
            Assert.AreNotEqual(0, _model.RobotsListProp[1].Route.Count);
            Assert.AreNotEqual(_model.RobotsListProp[1].DeliverdShelf, _model.RobotsListProp[0].DeliverdShelf);
            _model.MoveAtTick();
            _model.MoveAtTick();
            Assert.AreNotEqual(first, _model.RobotsListProp[0].Position);
            Assert.AreNotEqual(second, _model.RobotsListProp[1].Position);
        }

        [TestMethod]
        public void RobotPickUpShelf()
        {
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.MoveAtTick();//Utvonalat tervez
            _model.MoveAtTick();//Forog
            _model.MoveAtTick();//Lep
            _model.MoveAtTick();//Lep
            Assert.AreEqual(true, _model.RobotsListProp[0].ShelfOnTop); //Az elsõ robot már elérte a polcot
            Assert.AreEqual(false, _model.RobotsListProp[1].ShelfOnTop);//a második viszont még nem
            Assert.AreEqual('R', _model.Depo[_model.RobotsListProp[0].Position.X, _model.RobotsListProp[0].Position.Y]);
            Coordinate pos = _model.RobotsListProp[0].Position;
            _model.MoveAtTick();//Forog
            _model.MoveAtTick();//Lep
            Assert.AreEqual('0', _model.Depo[pos.X, pos.Y]);
        }

        public void RobotStation()
        {
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.MoveAtTick();//Utvonalat tervez
            _model.MoveAtTick();//Forog
            _model.MoveAtTick();//Lep
            _model.MoveAtTick();//Lep
            Assert.AreEqual(true, _model.RobotsListProp[0].ShelfOnTop); //Az elsõ robot már elérte a polcot
            Assert.AreEqual(false, _model.RobotsListProp[1].ShelfOnTop);//a második viszont még nem
            Assert.AreEqual('R', _model.Depo[_model.RobotsListProp[0].Position.X, _model.RobotsListProp[0].Position.Y]);
            Coordinate pos = _model.RobotsListProp[0].Position;
            _model.MoveAtTick();//Lep
            
            _model.MoveAtTick();//Lep
            Assert.AreEqual('0', _model.Depo[2, 2]);
            int a = 0;
            while (_model.RobotsListProp[0].Route.Count != 0)
            {
                _model.MoveAtTick();//Lep
                a++;
            }
            Assert.AreNotEqual(null, _model.StationsListProp.Find(s => s.Position.Equals(_model.RobotsListProp[0].Position)));
            Assert.AreEqual(0, _model.RobotsListProp[0].DeliverdShelf.Products.Count);
        }

        [TestMethod]
        public void RobotGoStation()
        {
            RobotStation();
        }

        public void RobotCharger()
        {
            _model.RobotsListProp[0].Energy = 8;
            _model.MoveAtTick();
            Coordinate chargerCoordinate = new Coordinate(0, 4);
            Assert.AreEqual(chargerCoordinate, _model.RobotsListProp[0].Route[^1]);
        }

        [TestMethod]
        public void RobotGoCharger()
        {
            RobotStation();
            RobotCharger();
        }
        [TestMethod]
        public void RobotGoBackToShelfAndPutItBack()
        {
            RobotStation();
            RobotCharger();
            
            while (_model.RobotsListProp[0].Route.Count != 0)
            {
                _model.MoveAtTick();//Lep
            }
            Assert.AreEqual(DestPoint.ToPickUpEmptyShelf, _model.RobotsListProp[0].ActualDestPoint);
            _model.MoveAtTick();
            while (_model.RobotsListProp[0].Route.Count != 0)
            {
                _model.MoveAtTick();//Lep
            }
            _model.MoveAtTick();
            _model.MoveAtTick();
            Assert.AreEqual(true, _model.RobotsListProp[0].ShelfOnTop);
            while (_model.RobotsListProp[0].Route.Count != 0)
            {
                _model.MoveAtTick();//Lep
            }
            //Ellenõrizzük, hogy a töltés után felvett polcot leszállítja e.
            Assert.AreEqual(new Coordinate(2,2), _model.RobotsListProp[0].Position);
        }

        [TestMethod]
        public void MoreThanOneRobotWorksInSameTime()
        {
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.MoveAtTick();//Utvonalat tervez
            Assert.AreNotEqual(0, _model.RobotsListProp[0].Route.Count);
            Assert.AreNotEqual(0, _model.RobotsListProp[1].Route.Count);
            //Elküldöm az elsõ robotot a hozzárendelt polcig
            //Természetesen közben a másik robot is halad
            while(_model.RobotsListProp[0].Route.Count != 0)
            {
                _model.MoveAtTick();
            }
            Assert.AreEqual(true, _model.RobotsListProp[0].ShelfOnTop);
            //Az elsõ robot elért a polcig, viszont a másikról még nem tudjuk, hogy odaért-e már
            //Ha még nem ért oda, akkor addig mozgatjuk a robotokat míg a másik robot is fel nem veszi a polcát
            if(_model.RobotsListProp[1].Route.Count != 0 && _model.RobotsListProp[1].ShelfOnTop == false)
            {
                while (_model.RobotsListProp[1].Route.Count != 0)
                {
                    _model.MoveAtTick();
                }
            }
            Assert.AreEqual(true, _model.RobotsListProp[1].ShelfOnTop);
            //Ezek után addig mozgatjuk õket amíg az összes termék le nem lesz szállítva
            Shelf s = _model.ShelvesListProp.Find(sf => sf.Products.Count != 0);
            Robot r = _model.RobotsListProp.Find(rb => rb.ShelfOnTop == true);
            while (s != null && r != null)
            {
                _model.MoveAtTick();
                s = _model.ShelvesListProp.Find(sf => sf.Products.Count != 0);
                r = _model.RobotsListProp.Find(rb => rb.ShelfOnTop == true);
            }
            _model.MoveAtTick();
            //Sikerült minden polcot leszállítani, tehát nincs olyan polc amin van termék
            Assert.AreEqual(null, _model.ShelvesListProp.Find(sf => sf.Products.Count != 0));
        }
        public void initializeForCollision()
        {
            _mockedTable = new Depot(7, 7);
            _mockedTable[0, 0] = 'D';
            _mockedTable[0, 1] = '0';
            _mockedTable[0, 2] = '0';
            _mockedTable[0, 3] = '0';
            _mockedTable[0, 4] = '0';
            _mockedTable[0, 5] = '0';
            _mockedTable[0, 6] = 'D';

            _mockedTable[1, 0] = '0';
            _mockedTable[1, 1] = '0';
            _mockedTable[1, 2] = '0';
            _mockedTable[1, 3] = '0';
            _mockedTable[1, 4] = '0';
            _mockedTable[1, 5] = '0';
            _mockedTable[1, 6] = '0';

            _mockedTable[2, 0] = '0';
            _mockedTable[2, 1] = '0';
            _mockedTable[2, 2] = 'P';
            _mockedTable.Products[2, 2].Add(2);
            _mockedTable[2, 3] = '0';
            _mockedTable[2, 4] = '0';
            _mockedTable[2, 5] = '0';
            _mockedTable[2, 6] = '0';

            _mockedTable[3, 0] = '0';
            _mockedTable[3, 1] = '0';
            _mockedTable[3, 2] = 'P';
            _mockedTable.Products[3, 2].Add(1);
            _mockedTable[3, 3] = '0';
            _mockedTable[3, 4] = '0';
            _mockedTable[3, 5] = '0';
            _mockedTable[3, 6] = '0';

            _mockedTable[4, 0] = '0';
            _mockedTable[4, 1] = 'R';
            _mockedTable[4, 2] = '0';
            _mockedTable[4, 3] = 'P';
            _mockedTable.Products[4, 2].Add(1);
            _mockedTable[4, 4] = '0';
            _mockedTable[4, 5] = '0';
            _mockedTable[4, 6] = '0';

            _mockedTable[5, 0] = '0';
            _mockedTable[5, 1] = 'R';
            _mockedTable[5, 2] = '0';
            _mockedTable[5, 3] = '0';
            _mockedTable[5, 4] = '0';
            _mockedTable[5, 5] = '0';
            _mockedTable[5, 6] = '0';

            _mockedTable[6, 0] = '0';
            _mockedTable[6, 1] = 'S';
            _mockedTable[6, 2] = '0';
            _mockedTable[6, 3] = 'S';
            _mockedTable[6, 4] = '0';
            _mockedTable[6, 5] = '0';
            _mockedTable[6, 6] = '0';

            _mock = new Mock<IDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(_mockedTable));

            _model = new ModelClass(7, 7, _mock.Object);
        }

        [TestMethod]
        public void CrossingCollision()
        {
            initializeForCollision();
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            //Mind a két robot kap egy alap útvonalat, hogy az ütközést tesztelni tudjuk
            List<Coordinate> firstRobotRoute = new List<Coordinate>();
            firstRobotRoute.Add(new Coordinate(5, 1));
            firstRobotRoute.Add(new Coordinate(6, 1));
            _model.RobotsListProp[0].Route.AddRange(firstRobotRoute);
            _model.RobotsListProp[0].DeliverdShelf = _model.ShelvesListProp[0];

            List<Coordinate> secondRobotRoute = new List<Coordinate>();
            secondRobotRoute.Add(new Coordinate(5, 2));
            secondRobotRoute.Add(new Coordinate(5, 3));
            _model.RobotsListProp[1].Route.AddRange(secondRobotRoute);
            _model.RobotsListProp[1].DeliverdShelf = _model.ShelvesListProp[1];

            //Megvizsgáljuk, hogy tényleg sikertült e beállítani a két robot útvonalát a következõ lépésekkel
            Assert.AreEqual(new Coordinate(5, 1), _model.RobotsListProp[0].NextStep());
            Assert.AreEqual(new Coordinate(5, 2), _model.RobotsListProp[1].NextStep());
            _model.MoveAtTick();
            Assert.IsFalse(_model.RobotsListProp[1].Route.Equals(secondRobotRoute));
            Assert.IsFalse(_model.RobotsListProp[0].Route.Equals(firstRobotRoute));
            Assert.IsFalse(_model.RobotsListProp[1].NextStep().Equals(_model.RobotsListProp[0].NextStep()));
        }

        public void initializeForCollision2()
        {
            //A
            _mockedTable = new Depot(7, 7);
            _mockedTable[0, 0] = 'D';
            _mockedTable[0, 1] = '0';
            _mockedTable[0, 2] = '0';
            _mockedTable[0, 3] = '0';
            _mockedTable[0, 4] = '0';
            _mockedTable[0, 5] = '0';
            _mockedTable[0, 6] = 'D';

            _mockedTable[1, 0] = '0';
            _mockedTable[1, 1] = '0';
            _mockedTable[1, 2] = '0';
            _mockedTable[1, 3] = '0';
            _mockedTable[1, 4] = '0';
            _mockedTable[1, 5] = '0';
            _mockedTable[1, 6] = '0';

            _mockedTable[2, 0] = '0';
            _mockedTable[2, 1] = '0';
            _mockedTable[2, 2] = 'P';
            _mockedTable.Products[2, 2].Add(2);
            _mockedTable[2, 3] = '0';
            _mockedTable[2, 4] = '0';
            _mockedTable[2, 5] = '0';
            _mockedTable[2, 6] = '0';

            _mockedTable[3, 0] = '0';
            _mockedTable[3, 1] = '0';
            _mockedTable[3, 2] = 'P';
            _mockedTable.Products[3, 2].Add(1);
            _mockedTable[3, 3] = '0';
            _mockedTable[3, 4] = '0';
            _mockedTable[3, 5] = '0';
            _mockedTable[3, 6] = '0';

            _mockedTable[4, 0] = '0';
            _mockedTable[4, 1] = '0';
            _mockedTable[4, 2] = 'P';
            _mockedTable[4, 3] = '0';
            _mockedTable.Products[4, 2].Add(1);
            _mockedTable[4, 4] = '0';
            _mockedTable[4, 5] = '0';
            _mockedTable[4, 6] = '0';

            _mockedTable[5, 0] = '0';
            _mockedTable[5, 1] = 'R';
            _mockedTable[5, 2] = '0';
            _mockedTable[5, 3] = 'R';
            _mockedTable[5, 4] = '0';
            _mockedTable[5, 5] = '0';
            _mockedTable[5, 6] = '0';

            _mockedTable[6, 0] = '0';
            _mockedTable[6, 1] = 'S';
            _mockedTable[6, 2] = '0';
            _mockedTable[6, 3] = 'S';
            _mockedTable[6, 4] = '0';
            _mockedTable[6, 5] = '0';
            _mockedTable[6, 6] = '0';

            _mock = new Mock<IDataAccess>();
            _mock.Setup(mock => mock.LoadAsync(It.IsAny<String>()))
                .Returns(() => Task.FromResult(_mockedTable));

            _model = new ModelClass(7, 7, _mock.Object);
        }

        [TestMethod]
        public void FrontalCollision()
        {
            initializeForCollision2();
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            //Mind a két robot kap egy alap útvonalat, hogy az ütközést tesztelni tudjuk
            List<Coordinate> firstRobotRoute = new List<Coordinate>();
            firstRobotRoute.Add(new Coordinate(5, 2));
            firstRobotRoute.Add(new Coordinate(5, 3));
            firstRobotRoute.Add(new Coordinate(5, 4));
            firstRobotRoute.Add(new Coordinate(5, 5));
            _model.RobotsListProp[0].Route.AddRange(firstRobotRoute);
            _model.RobotsListProp[0].DeliverdShelf = _model.ShelvesListProp[0];
            _model.RobotsListProp[0].Direction = Direction.Right;

            List<Coordinate> secondRobotRoute = new List<Coordinate>();
            secondRobotRoute.Add(new Coordinate(5, 2));
            secondRobotRoute.Add(new Coordinate(5, 1));
            secondRobotRoute.Add(new Coordinate(5, 0));
            _model.RobotsListProp[1].Route.AddRange(secondRobotRoute);
            _model.RobotsListProp[1].DeliverdShelf = _model.ShelvesListProp[1];
            _model.RobotsListProp[0].Direction = Direction.Left;

            //Megvizsgáljuk, hogy tényleg sikertült e beállítani a két robot útvonalát a következõ lépésekkel
            Assert.AreEqual(new Coordinate(5, 2), _model.RobotsListProp[0].NextStep());
            Assert.AreEqual(new Coordinate(5, 2), _model.RobotsListProp[1].NextStep());
            //Assert.IsTrue(_model.RobotsListProp[1].nextStep().Equals(_model.RobotsListProp[0].nextStep()));
            _model.MoveAtTick();
            _model.MoveAtTick();
            _model.MoveAtTick();
            Assert.IsFalse(_model.RobotsListProp[1].Route.Equals(secondRobotRoute));
            Assert.IsFalse(_model.RobotsListProp[0].Route.Equals(firstRobotRoute));
            Assert.IsFalse(_model.RobotsListProp[1].NextStep().Equals(_model.RobotsListProp[0].NextStep()));
        }

        [TestMethod]
        public void Rotate90()
        {
            initializeForCollision2();
            _model.Depo = _mockedTable;
            _model.MainInitializer();

            _model.RobotsListProp[0].Route.Add(new Coordinate(5, 2));
            Assert.AreEqual(Direction.Up, _model.RobotsListProp[0].Direction);
            _model.MoveAtTick();
            Assert.AreNotEqual(Direction.Up, _model.RobotsListProp[0].Direction);
            Assert.AreEqual(Direction.Right, _model.RobotsListProp[0].Direction);
        }

        [TestMethod]
        public void Rotate180()
        {
            initializeForCollision2();
            _model.Depo = _mockedTable;
            _model.MainInitializer();

            _model.RobotsListProp[0].Route.Add(new Coordinate(6, 1));
            Assert.AreEqual(Direction.Up, _model.RobotsListProp[0].Direction);
            _model.MoveAtTick();
            Assert.AreNotEqual(Direction.Down, _model.RobotsListProp[0].Direction);
            _model.MoveAtTick();
            Assert.AreEqual(Direction.Down, _model.RobotsListProp[0].Direction);
        }

        [TestMethod]
        public void ClosestShelf()
        {
            initializeForCollision2();
            _model.Depo = _mockedTable;
            _model.MainInitializer();

            Shelf s = _model.ClosestShelf(_model.RobotsListProp[0]);
            _model.MoveAtTick();
            Assert.AreEqual(s, _model.RobotsListProp[0].DeliverdShelf);
            Assert.AreNotEqual(new Coordinate(2,2), _model.RobotsListProp[0].DeliverdShelf);
        }

        [TestMethod]
        public void PickUpOrders()
        {
            initializeForCollision2();
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.Depo.Orders.Add(new Order { OrderTime = 0, ProductId = 1 });

            _model.MoveAtTick();
            Assert.AreEqual(1, _model.RobotsListProp[0].ShippingOrder);
            Assert.AreEqual(0, _model.Depo.Orders.Count);
        }


        [TestMethod]
        public void InitializeProducts()
        {
            initializeForCollision2();
            _model.Depo = _mockedTable;
            _model.MainInitializer();
            _model.GetAllProducts();
            Assert.AreEqual(2, _model.ProductsWithCount[1]);
            Assert.AreEqual(1, _model.ProductsWithCount[2]);
        }


        private void Model_End(object sender, EventArgs e)
        {
            Assert.AreEqual(null, _model.ShelvesListProp.Find(sf => sf.Products.Count != 0));
        }

    }
}
