CREATE TABLE api_lookup (
	id integer PRIMARY KEY,
	api_name varchar(255)
);

CREATE TABLE api_endpoint_lookup (
	id integer PRIMARY KEY,
	api_name_lookup integer REFERENCES api_lookup(id),
	api_endpoint_name varchar(255)
);

CREATE TABLE consumer_type_lookup (
	id integer PRIMARY KEY,
	type_name varchar(255)
);

CREATE TABLE tokens (
	id   integer PRIMARY KEY,
	api_name_lookup integer REFERENCES api_lookup(id),
	api_endpoint_lookup integer REFERENCES api_endpoint_lookup(id),
	environment varchar(255),
	consumer_name varchar(255),
	consumer_type_lookup integer REFERENCES consumer_type_lookup(id),
	requested_by varchar(255),
	authorized_by varchar(255),
	date_created timestamp NOT NULL,
	expiration_date timestamp,
	valid boolean NOT NULL
);