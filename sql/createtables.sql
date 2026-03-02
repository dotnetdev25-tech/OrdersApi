CREATE TABLE customers (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL,
    email TEXT NOT NULL UNIQUE
);
CREATE TABLE customer_type (
    id SERIAL PRIMARY KEY,
    type_name VARCHAR(100) NOT NULL UNIQUE,
    description TEXT
);
insert into customer_type(type_name, description) values('type1','type1desc');

ALTER TABLE customers
ADD COLUMN customer_type_id INT;

ALTER TABLE customers
ADD CONSTRAINT fk_customer_customer_type
FOREIGN KEY (customer_type_id)
REFERENCES customer_type(id);
ALTER TABLE customers
ALTER COLUMN customer_type_id SET DEFAULT 1;

CREATE TABLE products (
    product_id      serial PRIMARY KEY,
    product_name    text NOT NULL,
    sku             text UNIQUE,
    unit_price      numeric(10,2) NOT NULL,
    is_active       boolean NOT NULL DEFAULT true
);
CREATE TABLE order_items (
    order_id     int NOT NULL REFERENCES orders(order_id),
    line_number  int NOT NULL,
    product_id   int NOT NULL REFERENCES products(product_id),
    quantity     int NOT NULL,
    unit_price   numeric(10,2) NOT NULL,
    PRIMARY KEY (order_id, line_number)
);

create table order_type_dim (
    order_type_id serial primary key,
    order_type_name text not null unique
);
insert into order_type_dim (order_type_name)
values 
    ('online'),
    ('in_store'),
    ('subscription'),
    ('wholesale');


    CREATE TABLE order_status (
    order_status_id SERIAL PRIMARY KEY,
    status_name TEXT NOT NULL UNIQUE
);

CREATE TABLE payment_method (
    payment_method_id SERIAL PRIMARY KEY,
    method_name TEXT NOT NULL UNIQUE
);

CREATE TABLE fulfillment_method (
    fulfillment_method_id SERIAL PRIMARY KEY,
    method_name TEXT NOT NULL UNIQUE
);
ALTER TABLE orders
    ADD COLUMN order_status_id INT REFERENCES order_status(order_status_id),
    ADD COLUMN payment_method_id INT REFERENCES payment_method(payment_method_id),
    ADD COLUMN fulfillment_method_id INT REFERENCES fulfillment_method(fulfillment_method_id);
INSERT INTO order_status (status_name)
VALUES ('pending'), ('paid'), ('shipped'), ('cancelled'), ('refunded');

INSERT INTO payment_method (method_name)
VALUES ('credit_card'), ('ach'), ('cash'), ('check'), ('paypal');

INSERT INTO fulfillment_method (method_name)
VALUES ('pickup'), ('delivery'), ('ship');




CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    customer_id INT NOT NULL REFERENCES customers(id),
    order_date TIMESTAMP NOT NULL DEFAULT NOW()
);
alter table orders
add column order_type_id int references order_type_dim(order_type_id);


CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INT NOT NULL REFERENCES orders(id),
    product_name TEXT NOT NULL,
    quantity INT NOT NULL,
    unit_price NUMERIC(10,2) NOT NULL
);
