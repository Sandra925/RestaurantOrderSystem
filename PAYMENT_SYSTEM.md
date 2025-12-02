# Payment System Implementation

## Overview
A complete payment system has been implemented to ensure tables are only freed up after orders are paid. The system supports two payment methods: Card and Cash.

## Key Changes

### 1. **Order Model Updates** (`RestoranoSistema/Models/Order.cs`)
- Added `PaymentStatus` property (Unpaid, Paid, Cancelled)
- Added `PaymentMethod` property (Card, Cash)
- Added `PaidAt` timestamp to track when payment was made
- Default payment status is set to "Unpaid" when a new order is created

### 2. **Table Model Updates** (`RestoranoSistema/Models/Table.cs`)
- Updated `CurrentOrder` computed property to only return orders with `PaymentStatus == Unpaid`
- This ensures paid orders don't appear as the current active order

### 3. **Orders API Controller** (`RestoranoSistema/Controllers/OrdersController.cs`)
New endpoints added:

#### POST `/api/orders/{id}/pay`
Processes payment for an order
- **Parameters**: 
  - `id`: Order ID
  - `paymentMethod`: "Card" or "Cash"
- **Actions**:
  - Marks order as Paid
  - Records payment method and timestamp
  - Updates table status to Available
- **Response**: Updated order with payment details

#### GET `/api/orders/{id}/payment-status`
Retrieves payment status of an order
- **Parameters**: `id`: Order ID
- **Response**: Payment status, method, and timestamp

### 4. **Payment Page** (New)
**File**: `RestoranoSistema/Pages/Payment.cshtml` and `RestoranoSistema/Pages/Payment.cshtml.cs`

Features:
- Displays order summary with items and total amount
- Shows two payment options: Card and Cash
- Displays already-paid orders with payment details
- After successful payment, redirects back to table view
- Professional payment UI with payment method icons

**Route**: `/Payment?orderId={orderId}&tableId={tableId}`

### 5. **Table Page Updates** (`RestoranoSistema/Pages/Table.cshtml`)
- Shows "Process Payment" button when order is Ready but Unpaid
- Allows new orders to be created only after previous orders are paid
- Better handles table state management

### 6. **Database Migration**
**File**: `RestoranoSistema/Migrations/20251210120000_AddPaymentFieldsToOrders.cs`
- Adds three new columns to Orders table:
  - `PaymentStatus` (INTEGER, default 0 = Unpaid)
  - `PaymentMethod` (nullable INTEGER)
  - `PaidAt` (nullable DATETIME)

## Workflow

1. **Waiter creates order** for a table
   - Order status: Open
   - Payment status: Unpaid

2. **Kitchen prepares order**
   - Kitchen marks order as Ready
   - Order still shows on table

3. **Waiter initiates payment** (when customer wants to pay)
   - Navigates to Payment page
   - Selects payment method (Card or Cash)
   - Order marked as Paid
   - Table status becomes Available

4. **Table becomes available**
   - New orders can be created for this table
   - Paid order is no longer shown as active

## Authorization Policies
- **Payment endpoint**: Requires "CanCreateOrders" policy (Admin, Waiter)
- **View payment status**: Requires "CanViewOrders" policy (Admin, Waiter, Cook)

## Usage Example

### Process Payment (POST)
```bash
POST /api/orders/5/pay
Content-Type: application/json

{
  "paymentMethod": "Card"
}
```

### Check Payment Status (GET)
```bash
GET /api/orders/5/payment-status
```

## UI Flow

**Table View** ? **Order Management** ? **Payment Page** ? **Table Available**

Users can:
1. Click "Process Payment" button on Table page when order is ready
2. Select payment method (Card ?? or Cash ??)
3. See order summary with itemized list and total
4. Return to table view after payment

## Notes
- Tables remain "Occupied" until payment is made
- Orders maintain complete history even after payment
- Payment information is audit-able with timestamp and method
- The system prevents multiple concurrent unpaid orders on the same table
