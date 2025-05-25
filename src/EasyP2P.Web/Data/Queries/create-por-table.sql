-- Table: public.purchase_order_requests

-- DROP TABLE IF EXISTS public.purchase_order_requests;

CREATE TABLE IF NOT EXISTS public.purchase_order_requests
(
    id integer NOT NULL DEFAULT nextval('purchase_order_requests_id_seq'::regclass),
    item_name character varying(100) COLLATE pg_catalog."default" NOT NULL,
    quantity integer NOT NULL,
    comment character varying(500) COLLATE pg_catalog."default",
    request_date timestamp without time zone NOT NULL,
    requested_by character varying(100) COLLATE pg_catalog."default" NOT NULL,
    status character varying(20) COLLATE pg_catalog."default" NOT NULL,
    last_updated timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    updated_by character varying(100) COLLATE pg_catalog."default",
    justification text COLLATE pg_catalog."default" NOT NULL DEFAULT ''::text,
    priority character varying(20) COLLATE pg_catalog."default" NOT NULL DEFAULT 'Medium'::character varying,
    department character varying(100) COLLATE pg_catalog."default" NOT NULL DEFAULT ''::character varying,
    budget_code character varying(50) COLLATE pg_catalog."default",
    expected_delivery_date date,
    CONSTRAINT purchase_order_requests_pkey PRIMARY KEY (id)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.purchase_order_requests
    OWNER to postgres;
-- Index: idx_por_priority

-- DROP INDEX IF EXISTS public.idx_por_priority;

CREATE INDEX IF NOT EXISTS idx_por_priority
    ON public.purchase_order_requests USING btree
    (priority COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_por_status

-- DROP INDEX IF EXISTS public.idx_por_status;

CREATE INDEX IF NOT EXISTS idx_por_status
    ON public.purchase_order_requests USING btree
    (status COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_purchase_order_requests_department

-- DROP INDEX IF EXISTS public.idx_purchase_order_requests_department;

CREATE INDEX IF NOT EXISTS idx_purchase_order_requests_department
    ON public.purchase_order_requests USING btree
    (department COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_purchase_order_requests_priority

-- DROP INDEX IF EXISTS public.idx_purchase_order_requests_priority;

CREATE INDEX IF NOT EXISTS idx_purchase_order_requests_priority
    ON public.purchase_order_requests USING btree
    (priority COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_purchase_order_requests_status

-- DROP INDEX IF EXISTS public.idx_purchase_order_requests_status;

CREATE INDEX IF NOT EXISTS idx_purchase_order_requests_status
    ON public.purchase_order_requests USING btree
    (status COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

-- Trigger: update_purchase_order_requests_last_updated

-- DROP TRIGGER IF EXISTS update_purchase_order_requests_last_updated ON public.purchase_order_requests;

CREATE OR REPLACE TRIGGER update_purchase_order_requests_last_updated
    BEFORE UPDATE 
    ON public.purchase_order_requests
    FOR EACH ROW
    EXECUTE FUNCTION public.update_last_updated_column();