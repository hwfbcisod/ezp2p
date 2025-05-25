-- FUNCTION: public.update_last_updated_column()

-- DROP FUNCTION IF EXISTS public.update_last_updated_column();

CREATE OR REPLACE FUNCTION public.update_last_updated_column()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
BEGIN
    NEW.last_updated = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$BODY$;

ALTER FUNCTION public.update_last_updated_column()
    OWNER TO postgres;
