# RestaurantOrderSystem
## 1. System Purpose
The goal of this project is to create a restaurant management system that simplifies the daily work of restaurant staff.  
It makes order processing faster and more organized.  

The system includes a **restaurant map** where the administrator can add new tables as objects.  
The bartender can take a new order by clicking on an existing table. A form then appears with:  
- The table number (displayed automatically).  
- An input field for the number of guests.  

After confirming the form, an empty order is created.  
The order window contains a menu list from which guests can choose their dishes and drinks.  
When the order is confirmed:  
- A food ticket is sent to the **kitchen**.  
- A drinks ticket is sent to the **bar**.  

---

## 2. Functional Requirements

### Administrator can:
- Create new tables.  
- Remove dishes from an order (with or without write-off).  
- Close the general shift.  
- View closed checks.  

### Bartender can:
- Accept new orders.  
- Move an order to another table.  
- Split a table (e.g., 1.1, 1.2 …).  
- Complete an order (mark it as paid in cash or by card).  
- Choose different restaurant halls with unique table layouts.  

### Chef can:
- Mark order tickets as completed.  

---

## 3. System Architecture
The system consists of the following parts:  
- **Razor Pages** – server side (Back-End).  
- **SQLite** – project database.  
- **HTML / CSS / JavaScript** – client side (Front-End).  

---

## 4. Technologies Used
- ASP.NET Razor Pages  
- SQLite  
- HTML  
- CSS  
- JavaScript  

---

