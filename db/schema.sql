-- Pica Pollo Rey POS - Commit 2 (SQLite)

CREATE TABLE IF NOT EXISTS Producto (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nombre TEXT NOT NULL,
    Categoria TEXT NOT NULL,
    Precio REAL NOT NULL,
    Activo INTEGER NOT NULL DEFAULT 1
);

CREATE TABLE IF NOT EXISTS Venta (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Fecha TEXT NOT NULL,
    MetodoPago TEXT NOT NULL,
    Subtotal REAL NOT NULL,
    Itbis REAL NOT NULL,
    Total REAL NOT NULL
);

CREATE TABLE IF NOT EXISTS VentaDetalle (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    VentaId INTEGER NOT NULL,
    ProductoId INTEGER NOT NULL,
    NombreProducto TEXT NOT NULL,
    PrecioUnitario REAL NOT NULL,
    Cantidad INTEGER NOT NULL,
    TotalLinea REAL NOT NULL,
    FOREIGN KEY (VentaId) REFERENCES Venta(Id)
);

-- SEED (solo si no hay productos)
INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Combo Pollo 2 piezas', 'Combos', 250, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;

INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Combo Pollo 3 piezas', 'Combos', 320, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;

INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Pollo 1 pieza', 'Pollo', 120, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;

INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Pollo 2 piezas', 'Pollo', 220, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;

INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Papas Fritas', 'Acompañantes', 90, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;

INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Tostones', 'Acompañantes', 90, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;

INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Refresco', 'Bebidas', 70, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;

INSERT INTO Producto (Nombre, Categoria, Precio, Activo)
SELECT 'Agua', 'Bebidas', 40, 1
WHERE (SELECT COUNT(*) FROM Producto) = 0;
