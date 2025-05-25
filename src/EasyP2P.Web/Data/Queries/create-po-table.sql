-- Table: public.purchase_orders

-- DROP TABLE IF EXISTS public.purchase_orders;

CREATE TABLE IF NOT EXISTS public.purchase_orders
(
    id integer NOT NULL DEFAULT nextval('purchase_orders_id_seq'::regclass),
    purchase_order_request_id integer NOT NULL,
    item_name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    quantity integer NOT NULL,
    unit_price numeric(19,4) NOT NULL,
    total_price numeric(19,4) NOT NULL,
    supplier character varying(100) COLLATE pg_catalog."default" NOT NULL,
    order_date timestamp without time zone NOT NULL,
    created_by character varying(100) COLLATE pg_catalog."default" NOT NULL,
    status character varying(20) COLLATE pg_catalog."default" NOT NULL,
    CONSTRAINT purchase_orders_pkey PRIMARY KEY (id),
    CONSTRAINT fk_purchase_order_request FOREIGN KEY (purchase_order_request_id)
        REFERENCES public.purchase_order_requests (id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE RESTRICT
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.purchase_orders
    OWNER to postgres;
-- Index: idx_purchase_orders_request_id

-- DROP INDEX IF EXISTS public.idx_purchase_orders_request_id;

CREATE INDEX IF NOT EXISTS idx_purchase_orders_request_id
    ON public.purchase_orders USING btree
    (purchase_order_request_id ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_purchase_orders_status

-- DROP INDEX IF EXISTS public.idx_purchase_orders_status;

CREATE INDEX IF NOT EXISTS idx_purchase_orders_status
    ON public.purchase_orders USING btree
    (status COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;