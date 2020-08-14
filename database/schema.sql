CREATE TABLE api_lookup (
	id int PRIMARY KEY,
	api_name varchar(255)
);

CREATE TABLE api_endpoint_lookup (
	id int PRIMARY KEY,
	api_lookup_id int REFERENCES api_lookup(id),
	api_endpoint_name varchar(255)
);

CREATE TABLE consumer_type_lookup (
	id int PRIMARY KEY,
	type_name varchar(255)
);
-- auto generate primary keys for table
CREATE TABLE tokens (
	id SERIAL PRIMARY KEY,
	api_lookup_id int REFERENCES api_lookup(id),
	api_endpoint_lookup_id int REFERENCES api_endpoint_lookup(id),
	environment varchar(255),
	consumer_name varchar(255),
	consumer_type_lookup int REFERENCES consumer_type_lookup(id),
	requested_by varchar(255),
	authorized_by varchar(255),
	date_created timestamp NOT NULL,
	expiration_date timestamp NULL,
	enabled boolean NOT NULL
);