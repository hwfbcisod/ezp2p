-- Create the purchase_orders table with PostgreSQL syntax

CREATE TABLE purchase_orders (
    id SERIAL PRIMARY KEY,
    purchase_order_request_id INTEGER NOT NULL,
    item_name VARCHAR(100) NOT NULL,
    quantity INTEGER NOT NULL,
    unit_price DECIMAL(19, 4) NOT NULL,
    total_price DECIMAL(19, 4) NOT NULL,
    supplier VARCHAR(100) NOT NULL,
    order_date TIMESTAMP NOT NULL,
    created_by VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL,
    CONSTRAINT fk_purchase_order_request
        FOREIGN KEY (purchase_order_request_id)
        REFERENCES purchase_order_requests (id)
        ON DELETE RESTRICT
);

-- Create indexes
CREATE INDEX idx_purchase_orders_status ON purchase_orders(status);
CREATE INDEX idx_purchase_orders_request_id ON purchase_orders(purchase_order_request_id);