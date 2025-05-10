-- Create the purchase_order_requests table with PostgreSQL syntax

CREATE TABLE purchase_order_requests (
    id SERIAL PRIMARY KEY,
    item_name VARCHAR(100) NOT NULL,
    quantity INTEGER NOT NULL,
    comment VARCHAR(500),
    request_date TIMESTAMP NOT NULL,
    requested_by VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL
);

-- Create an index for faster querying by status
CREATE INDEX idx_purchase_order_requests_status ON purchase_order_requests(status);

-- Add some sample data (optional)
INSERT INTO purchase_order_requests (item_name, quantity, comment, request_date, requested_by, status)
VALUES 
    ('Office Supplies', 10, 'Needed for the marketing department', NOW() - INTERVAL '2 days', 'John Doe', 'Pending'),
    ('Laptop', 2, 'For new developers', NOW() - INTERVAL '1 day', 'Jane Smith', 'Pending'),
    ('Conference Room Equipment', 1, 'Projector and screen', NOW() - INTERVAL '5 hours', 'Robert Johnson', 'Pending');