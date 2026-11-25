# Fixed-width record importer

Import legacy records into accepted/rejected review streams with source row numbers.

Language: **Visual Basic .NET**.

[![Validation](https://github.com/Michaelhoang8/vbnet-fixed-width-importer/actions/workflows/ci.yml/badge.svg)](https://github.com/Michaelhoang8/vbnet-fixed-width-importer/actions/workflows/ci.yml)

## Run

```sh
dotnet run -- fixtures/records.txt
```

## Behavior

- Each record is exactly 21 ASCII characters: four-digit ID, eight-digit YYYYMMDD date, sign, eight-digit integer cents.
- Rejects malformed dates, fields, lengths and duplicate accepted identifiers.
- A rejected record does not reserve its identifier. Output is structured JSON.

## Validation

```sh
dotnet run -- --test
```

The workflow executes the checked-in fixtures and boundary tests. See the workflow log for the actual result; tests are included with the source.

## Scope

The format is deliberately fixed and documented; it is not a general EDI parser. Zero amounts are valid. All data are small constructed fixtures.

Code: [MIT](LICENSE). Fixtures are constructed examples.
