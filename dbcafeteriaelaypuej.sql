CREATE DATABASE cafeteria_elay_puej;
USE cafeteria_elay_puej;

CREATE TABLE Users (
    id INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(150) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name VARCHAR(200) NOT NULL,
    role ENUM('admin', 'worker', 'customer') NOT NULL DEFAULT 'customer',
    phone VARCHAR(20),
    concurrency_stamp CHAR(36) NOT NULL DEFAULT (UUID()),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE Categories (
    id INT PRIMARY KEY AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    icon VARCHAR(50),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Products (
    id INT PRIMARY KEY AUTO_INCREMENT,
    category_id INT NOT NULL,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    price DECIMAL(10,2) NOT NULL,
    preparation_time VARCHAR(50),
    origin VARCHAR(200),
    flavor_notes VARCHAR(255),
    image_url VARCHAR(500),
    stock INT NOT NULL DEFAULT 0,
    min_stock INT DEFAULT 5,
    is_available BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (category_id) REFERENCES Categories(id) ON DELETE RESTRICT
);

CREATE TABLE Inventory_Movements (
    id INT PRIMARY KEY AUTO_INCREMENT,
    product_id INT NOT NULL,
    movement_type ENUM('entry', 'exit', 'adjustment') NOT NULL,
    quantity INT NOT NULL,
    reason VARCHAR(255),
    user_id INT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (product_id) REFERENCES Products(id) ON DELETE RESTRICT,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE RESTRICT
);

CREATE TABLE Orders (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    order_number VARCHAR(20) UNIQUE NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    tax DECIMAL(10,2) DEFAULT 0.00,
    total DECIMAL(10,2) NOT NULL,
    payment_method ENUM('qr', 'cash', 'card') NOT NULL,
    payment_status ENUM('pending', 'paid', 'cancelled') DEFAULT 'pending',
    order_status ENUM('pending', 'preparing', 'ready', 'delivered') DEFAULT 'pending',
    pickup_time DATETIME,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE RESTRICT
);

CREATE TABLE Order_Details (
    id INT PRIMARY KEY AUTO_INCREMENT,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    subtotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (order_id) REFERENCES Orders(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES Products(id) ON DELETE RESTRICT
);

CREATE TABLE QR_Payments (
    id INT PRIMARY KEY AUTO_INCREMENT,
    order_id INT NOT NULL,
    qr_image_url VARCHAR(500) NOT NULL,
    amount DECIMAL(10,2) NOT NULL,
    payment_reference VARCHAR(100),
    verified_at TIMESTAMP NULL,
    verified_by INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (order_id) REFERENCES Orders(id) ON DELETE CASCADE,
    FOREIGN KEY (verified_by) REFERENCES Users(id) ON DELETE SET NULL
);

CREATE TABLE Audit_Logs (
    id INT PRIMARY KEY AUTO_INCREMENT,
    user_id INT NOT NULL,
    action VARCHAR(100) NOT NULL,
    details JSON,
    ip_address VARCHAR(45),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES Users(id) ON DELETE RESTRICT
);

CREATE INDEX idx_users_email ON Users(email);
CREATE INDEX idx_users_role ON Users(role);
CREATE INDEX idx_products_category ON Products(category_id);
CREATE INDEX idx_products_name ON Products(name);
CREATE INDEX idx_products_available ON Products(is_available);
CREATE INDEX idx_orders_user ON Orders(user_id);
CREATE INDEX idx_orders_number ON Orders(order_number);
CREATE INDEX idx_orders_status ON Orders(order_status);
CREATE INDEX idx_orders_payment ON Orders(payment_status);
CREATE INDEX idx_orders_created ON Orders(created_at);
CREATE INDEX idx_order_details_order ON Order_Details(order_id);
CREATE INDEX idx_inventory_product ON Inventory_Movements(product_id);
CREATE INDEX idx_inventory_created ON Inventory_Movements(created_at);
CREATE INDEX idx_audit_user ON Audit_Logs(user_id);
CREATE INDEX idx_audit_created ON Audit_Logs(created_at);

INSERT INTO Categories (name, description, icon) VALUES
('Cafés', 'Cafés de origen único y blends especiales', 'coffee'),
('Desayunos', 'Desayunos completos para empezar el día', 'breakfast'),
('Repostería', 'Pastelería artesanal y panadería', 'bakery'),
('Bebidas Frías', 'Refrescos naturales y smoothies', 'cold-drinks');

-- Admin123!
USE cafeteria_elay_puej;

INSERT INTO Users (
    Email,
    PasswordHash,
    FullName,
    Role,
    Phone,
    ConcurrencyStamp,
    IsActive,
    IsEmailVerified,
    EmailVerifiedAt,
    CreatedAt,
    UpdatedAt
) VALUES (
    'admin@elay.com',
    '$2a$11$Ax0AGgwV.oV.pmKvHs8V9eeHCUAUlOXWJLwZNdRtuAcAJJ7JRGMK.',
    'Administrador Elay Puej',
    'admin',
    '777-888-999',
    UUID(),
    1,
    1,
    NOW(),
    NOW(),
    NOW()
);

select * from products;
delete from Users where id = 5;