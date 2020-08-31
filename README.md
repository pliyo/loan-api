# loan-api

Loan api will have a record for each customer account number.

Once a payment is received, it will update it's loan balance.

If a loan is finished it will send Loan finished event.
If a loan is not finished it will send loan updated + updated daily usage.

# Testing

I focused on unit tests because that's what it's certain about this problem, as the system relies heavily on integration with other services it will need plenty of focus
around integration testing.

# Assumptions

- Deduplication of messages is handled somewhere else.
- Data is in memory to make it easier.
- Assume we keep track of how much you have payed already.

# Questions

- What do we do with payments for loans that don't exists?
- What happen if the payment comes for a loan that it's already payed?
