import { BrowserRouter, Route, Routes } from "react-router";
import Layout from "./Layout.jsx";
import Home from "./pages/Home.jsx";
import Register from "./pages/Register.jsx";
import Login from "./pages/Login.jsx";
import EmployeeView from "./pages/admin/EmployeeView.jsx";
import NewEmployee from "./pages/admin/NewEmployee.jsx";
import IssueForm from "./pages/issue/IssueForm.jsx";
import EditForm from "./pages/admin/EditForm.jsx";
import IssueView from "./pages/employee/IssueView.jsx";
import CustomerChat from "./pages/customer/CustomerChat.jsx";
import EmployeeChat from "./pages/employee/EmployeeChat.jsx";
import EmployeeCheck from "./pages/protection/EmployeeCheck.jsx";
import NoPage from "./pages/NoPage.jsx";
import AdminCheck from "./pages/protection/AdminCheck.jsx";
import CustomerCheck from "./pages/protection/CustomerCheck.jsx";

export default function Router() {
  return <>
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />} >
          <Route index element={<Home />} />
          <Route path="/register" element={<Register />} />
          <Route path="/login" element={<Login />} />
          <Route path="/:company_name/issueform" element={<IssueForm />} />
          <Route path="/employee" element={<EmployeeCheck />}>
            <Route path="issues" element={<IssueView />} />
            <Route path="chat/:issue_id" element={<EmployeeChat />} />
          </Route>
          <Route path="/admin" element={<AdminCheck />}>
            <Route path="employees" element={<EmployeeView />} />
            <Route path="employees/new" element={<NewEmployee />} />
            <Route path="form/edit" element={<EditForm />} />
          </Route>
          <Route path="*" element={<NoPage />} />
        </Route>
        <Route path="/chat" element={<CustomerCheck />}>
          <Route path=":issue_id" element={<CustomerChat />} />
        </Route>
      </Routes>
    </BrowserRouter>
  </>
}
