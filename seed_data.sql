--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: postgres
--
pg_dump --data-only --inserts --table=your_table_name your_database > your_table_inserts.sql


COPY public.customers (id, name, email, customer_type_id) FROM stdin;
12      newOne  newone@gmail.com        1
13      hateoas hateoas@gmail.com       1
14      hateoas2        hateoas2@gmail.com      1
15      aichanges       aichgs@example.com      1
4       john theisen    johnjtheisen14@gmail.com        1
10      logged in       logged@gmail.com        1
11      dummy2  dummy2@gmail.com        1
6       name666 email6@wherever.com     1
1       patch2aaaa      jt@gmail.com    1
\
pg_dump --data-only --inserts --table=orders orders_demo > /var/tmp/orders_data.sql
--
-- Data for Name: orders; Type: TABLE DATA; Schema: public; Owner: postgres
--

INSERT INTO public.orders VALUES (20, 1, '2026-02-04 10:48:16.291262', 5.00, 1,>
INSERT INTO public.orders VALUES (22, 1, '2026-02-04 11:34:26.256259', 20.00, 1>
INSERT INTO public.orders VALUES (23, 1, '2026-02-05 05:16:50.518674', 99.00, 1>
INSERT INTO public.orders VALUES (24, 1, '2026-02-05 06:50:31.015545', 50.00, 1>
INSERT INTO public.orders VALUES (1, 1, '2026-01-05 07:19:31.945464', 99.00, 1,>
INSERT INTO public.orders VALUES (4, 1, '2026-01-28 08:22:04.38635', 50.00, 1, >
INSERT INTO public.orders VALUES (3, 1, '2026-01-27 10:24:34.332671', 1.00, 1, >
INSERT INTO public.orders VALUES (6, 1, '2026-01-30 09:11:13.651484', 0.00, 1, >
INSERT INTO public.orders VALUES (7, 1, '2026-01-30 09:11:14.899267', 0.00, 1, >
INSERT INTO public.orders VALUES (8, 1, '2026-01-30 09:11:15.966799', 0.00, 1, >
INSERT INTO public.orders VALUES (10, 1, '2026-01-30 09:44:19.555252', 44.00, 1>
INSERT INTO public.orders VALUES (12, 1, '2026-01-30 12:28:05.544771', -1.00, 1>
INSERT INTO public.orders VALUES (13, 1, '2026-02-02 12:29:59.148814', 5.00, 1,>

maybe write to /var/lib/postgresql 

all commands
udo -u postgres pg_dump -d orders_demo > /tmp/backup.sql
pg_dump -U postgres -d orders_demo > ~/my_dump.sql
pg_dump your_db_name > /var/tmp/my_dump.sql
pg_dump -a -t customers -d orders_demo > /var/tmp/customers_inserts.sql
// the postgres user scenario
sudo -u postgres createuser --interactive
# Enter your Ubuntu username when prompted.
# Say 'y' to making it a superuser.