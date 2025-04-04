import EmployeeCard from "./EmployeeCard.jsx";

export default function EmployeeList({ employeeList, getEmployees }) {
    return <div className="list">
        <div className="employeeListHeaders">
            <p>Firstname</p>
            <p>Lastname</p>
            <p>Email</p>
            <p>ROLE</p>
            <p>Username</p>
        </div>
        {employeeList.map(employee => <EmployeeCard key={employee.id} employee={employee} getEmployees={getEmployees} />)}
    </div>
}
