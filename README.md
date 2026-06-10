# REDINGTON Probability Calculator

A small C# and React coding test project for calculating probabilities using two supported operations:

- `CombinedWith`: $P(A) \times P(B)$
- `Either`: $P(A) + P(B) - P(A) \times P(B)$

## Purpose

This repository is being set up as a lightweight but professional solution that demonstrates:

- clear structure
- maintainable code
- validation at the boundary
- domain modelling
- testability
- practical scalability thinking

## Planned Solution

The intended solution is:

- a React frontend for entering two probabilities, selecting a calculation type, and showing the result
- an ASP.NET Core backend for validation and calculation execution
- a small domain model centered on a `Probability` value object
- unit tests and targeted API/frontend tests
- simple file-based audit logging for the coding test

## Planned Architecture

The target architecture is intentionally lightweight:

- frontend: React with TypeScript
- backend: ASP.NET Core Minimal API in C#
- domain: probability value object plus calculation strategies
- validation: request validation at the API boundary plus domain invariants
- operations: simple logging, container support, and CI as follow-on work

## Notes

- This project is intended to stay simple for the exercise and avoid unnecessary infrastructure such as authentication, a database, or microservices.
- The design will still aim to show how the solution could evolve for higher scale, reliability, and internal enterprise use.
  