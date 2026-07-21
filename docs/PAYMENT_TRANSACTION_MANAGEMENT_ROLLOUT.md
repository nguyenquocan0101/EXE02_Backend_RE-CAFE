# Payment transaction management rollout

## Release gate

Do not release until the backend integration suite, FE production build, production dependency audit, bundle secret scan, and an authenticated browser smoke test of `/admin/payments` all pass. The browser smoke remains a human gate when no browser session is available in automation.

## Pre-deploy checks

1. Take a PostgreSQL backup and confirm it can be restored.
2. Verify every `Payments.OrderId` still resolves to one order and review duplicate settlement rows before migration.
3. Run:

   ```powershell
   dotnet test tests\EXE02_Backend_RE-CAFE.Tests\EXE02_Backend_RE-CAFE.Tests.csproj -c Release
   dotnet list EXE02_Backend_RE-CAFE.csproj package --vulnerable --include-transitive
   ```

4. In the FE repository, run `npm run build` and `npm audit --omit=dev`.
5. Confirm the generated FE bundle contains no SePay credential, `VITE_SEPAY_DEV_API_KEY`, simulator copy, or simulator action.

## Deployment order

1. Apply `20260721053802_AddPaymentTransactionManagement` while the previous API is still serving traffic. The migration is additive: it adds `Payments.CreatedAt` plus query indexes.
2. Deploy the backend and verify list/detail/summary/export authorization for Admin and Staff; Customer must receive 403 and anonymous requests 401.
3. Deploy the FE and smoke `/admin/payments`: loading, populated list, combined filters, pagination, detail, summary, CSV export, empty result, API error, and Staff read-only navigation.
4. Smoke customer checkout with COD and bank transfer, profile payment of an unpaid bank-transfer order, successful webhook, replayed webhook, underpayment, and polling failure/recovery.
5. Keep export disabled at the edge or hide its FE entry if the production-like p95 exceeds 500 ms or the 10,000-row rejection contract fails.

## Monitoring

For at least the first release window, alert on:

- increases in payment API 401/403, 5xx, and p95 list latency;
- rejected exports and exports close to the 10,000-row ceiling;
- duplicate webhook references, underpayments, or divergent `Orders.PaymentStatus` and `Payments.Status`;
- failed `view_list`, `view_detail`, or `export` audit writes;
- unexpected payment totals compared with paid-only summary queries.

Do not expose the Vite development server beyond localhost. The current Vite 5 toolchain inherits an esbuild development-server advisory; removing it requires a separately tested major Vite upgrade. Production dependencies must remain at zero known vulnerabilities.

## Rollback

1. Disable the FE payment route/export entry first.
2. Roll back the API image while keeping the additive migration in place; the previous API does not depend on the new column or indexes.
3. Restore the database only for confirmed data corruption. Do not run the migration `Down` during a routine application rollback because dropping `CreatedAt` removes collected transaction metadata.
4. Preserve audit records and webhook diagnostics for incident review; never copy raw credentials or full webhook payloads into tickets or logs.
