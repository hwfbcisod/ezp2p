---- Create suppliers table
--CREATE TABLE suppliers (
--    id SERIAL PRIMARY KEY,
--    name VARCHAR(200) NOT NULL,
--    contact_person VARCHAR(100),
--    email VARCHAR(100),
--    phone VARCHAR(20),
--    address VARCHAR(300),
--    city VARCHAR(100),
--    state VARCHAR(100),
--    country VARCHAR(100),
--    postal_code VARCHAR(20),
--    tax_id VARCHAR(50),
--    payment_terms VARCHAR(100),
--    status VARCHAR(20) NOT NULL DEFAULT 'Active',
--    rating INTEGER CHECK (rating >= 1 AND rating <= 5),
--    website VARCHAR(200),
--    notes TEXT,
--    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
--    created_by VARCHAR(100) NOT NULL,
--    last_updated TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
--    updated_by VARCHAR(100) NOT NULL,
    
--    CONSTRAINT unique_supplier_name UNIQUE(name),
--    CONSTRAINT valid_email CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$' OR email IS NULL),
--    CONSTRAINT valid_status CHECK (status IN ('Active', 'Inactive', 'Pending', 'Suspended'))
--);

---- Create indexes for better performance
--CREATE INDEX idx_suppliers_name ON suppliers(name);
--CREATE INDEX idx_suppliers_status ON suppliers(status);
--CREATE INDEX idx_suppliers_city ON suppliers(city);
--CREATE INDEX idx_suppliers_created_date ON suppliers(created_date);

---- Insert sample data
--INSERT INTO suppliers (name, contact_person, email, phone, address, city, state, country, postal_code, tax_id, payment_terms, status, rating, website, notes, created_by, updated_by) VALUES
--('TechCorp Solutions', 'John Smith', 'john.smith@techcorp.com', '+1-555-0123', '123 Technology Blvd', 'San Francisco', 'CA', 'USA', '94105', 'TAX123456789', 'Net 30', 'Active', 5, 'https://techcorp.com', 'Reliable technology supplier with excellent customer service', 'System', 'System'),

--('Office Supplies Plus', 'Sarah Johnson', 'sarah@officesupplies.com', '+1-555-0234', '456 Business Park Dr', 'Chicago', 'IL', 'USA', '60601', 'TAX987654321', 'Net 15', 'Active', 4, 'https://officesupplies.com', 'Large office supplies vendor with competitive pricing', 'System', 'System'),

--('Global Manufacturing Co', 'Michael Chen', 'mchen@globalmanuf.com', '+1-555-0345', '789 Industrial Way', 'Detroit', 'MI', 'USA', '48201', 'TAX456789123', 'Net 45', 'Active', 4, 'https://globalmanuf.com', 'Manufacturing equipment and parts supplier', 'System', 'System'),

--('European Import Ltd', 'Anna Mueller', 'a.mueller@europeimport.com', '+49-30-12345678', 'Hauptstraße 100', 'Berlin', 'Berlin', 'Germany', '10115', 'DE123456789', 'Net 30', 'Active', 3, 'https://europeimport.com', 'European supplier for specialized components', 'System', 'System'),

--('Local Services Inc', 'David Brown', 'dbrown@localservices.com', '+1-555-0456', '321 Service Street', 'Austin', 'TX', 'USA', '73301', 'TAX789123456', 'Net 15', 'Pending', 3, 'https://localservices.com', 'New supplier under evaluation', 'System', 'System'),

--('Furniture & Fixtures Co', 'Lisa Williams', 'lwilliams@furniture.com', '+1-555-0567', '654 Design Avenue', 'Los Angeles', 'CA', 'USA', '90210', 'TAX321654987', 'Net 30', 'Active', 5, 'https://furniture.com', 'High-quality office furniture supplier', 'System', 'System'),

--('Construction Materials Ltd', 'Robert Taylor', 'rtaylor@construction.com', '+1-555-0678', '987 Builder Road', 'Houston', 'TX', 'USA', '77001', 'TAX654987321', 'Net 60', 'Inactive', 2, 'https://construction.com', 'Construction and building materials - currently not active', 'System', 'System'),

--('IT Solutions Group', 'Jennifer Davis', 'jdavis@itsolutions.com', '+1-555-0789', '147 Digital Plaza', 'Seattle', 'WA', 'USA', '98101', 'TAX147258369', 'Net 30', 'Active', 4, 'https://itsolutions.com', 'Enterprise IT solutions and software licensing', 'System', 'System');