-- Table: public.users

-- DROP TABLE IF EXISTS public.users;

CREATE TABLE IF NOT EXISTS public.users
(
    id character varying(36) COLLATE pg_catalog."default" NOT NULL,
    username character varying(256) COLLATE pg_catalog."default",
    normalized_username character varying(256) COLLATE pg_catalog."default",
    email character varying(256) COLLATE pg_catalog."default",
    normalized_email character varying(256) COLLATE pg_catalog."default",
    email_confirmed boolean NOT NULL DEFAULT false,
    password_hash text COLLATE pg_catalog."default",
    security_stamp character varying(36) COLLATE pg_catalog."default",
    phone_number character varying(50) COLLATE pg_catalog."default",
    phone_number_confirmed boolean NOT NULL DEFAULT false,
    two_factor_enabled boolean NOT NULL DEFAULT false,
    lockout_end timestamp without time zone,
    lockout_enabled boolean NOT NULL DEFAULT true,
    access_failed_count integer NOT NULL DEFAULT 0,
    first_name character varying(100) COLLATE pg_catalog."default",
    last_name character varying(100) COLLATE pg_catalog."default",
    department character varying(100) COLLATE pg_catalog."default",
    role character varying(50) COLLATE pg_catalog."default" NOT NULL DEFAULT 'Requestor'::character varying,
    created_date timestamp without time zone NOT NULL DEFAULT now(),
    is_active boolean NOT NULL DEFAULT true,
    CONSTRAINT users_pkey PRIMARY KEY (id),
    CONSTRAINT users_normalized_email_key UNIQUE (normalized_email),
    CONSTRAINT users_normalized_username_key UNIQUE (normalized_username)
)

TABLESPACE pg_default;

ALTER TABLE IF EXISTS public.users
    OWNER to postgres;
-- Index: idx_users_normalized_email

-- DROP INDEX IF EXISTS public.idx_users_normalized_email;

CREATE INDEX IF NOT EXISTS idx_users_normalized_email
    ON public.users USING btree
    (normalized_email COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_users_normalized_username

-- DROP INDEX IF EXISTS public.idx_users_normalized_username;

CREATE INDEX IF NOT EXISTS idx_users_normalized_username
    ON public.users USING btree
    (normalized_username COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;
-- Index: idx_users_role

-- DROP INDEX IF EXISTS public.idx_users_role;

CREATE INDEX IF NOT EXISTS idx_users_role
    ON public.users USING btree
    (role COLLATE pg_catalog."default" ASC NULLS LAST)
    TABLESPACE pg_default;

INSERT INTO users (
    id, 
    username, 
    normalized_username, 
    email, 
    normalized_email, 
    email_confirmed, 
    password_hash, 
    security_stamp, 
    phone_number, 
    phone_number_confirmed, 
    two_factor_enabled, 
    lockout_end, 
    lockout_enabled, 
    access_failed_count, 
    first_name, 
    last_name, 
    department, 
    role, 
    created_date, 
    is_active
) VALUES (
    gen_random_uuid()::text,                               -- id (plain GUID)
    'admin@easyp2p.com',                                   -- username
    'ADMIN@EASYP2P.COM',                                   -- normalized_username
    'admin@easyp2p.com',                                   -- email
    'ADMIN@EASYP2P.COM',                                   -- normalized_email
    true,                                                  -- email_confirmed
    NULL,                                                  -- password_hash (will be NULL)
    gen_random_uuid()::text,                              -- security_stamp (plain GUID)
    NULL,                                                  -- phone_number
    false,                                                 -- phone_number_confirmed
    false,                                                 -- two_factor_enabled
    NULL,                                                  -- lockout_end
    true,                                                  -- lockout_enabled
    0,                                                     -- access_failed_count
    'Super',                                               -- first_name
    'Admin',                                               -- last_name
    'IT',                                                  -- department
    'Administrator',                                       -- role
    NOW(),                                                 -- created_date
    true                                                   -- is_active
)
ON CONFLICT (normalized_email) DO NOTHING;