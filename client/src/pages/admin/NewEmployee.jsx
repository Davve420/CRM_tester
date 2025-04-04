export default function NewEmployee() {
    
    async function addEmployee(formdata) {
        const response = await fetch(`/api/users/create`, {
            method: "post",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                firstname: formdata.get("firstname"),
                lastname: formdata.get("lastname"),
                email: formdata.get("email"),
                password: formdata.get("password"),
                role: formdata.get("role")
            })
        })

        const result = await response.json()

        if (response.ok) {
            alert(`${formdata.get("firstname")} ${formdata.get("lastname")} har registrerats.\nEtt bekräftnings mejl har skickats till ${formdata.get("email")}.`)
        } else {
            alert(result.message)
        }
    }

    return <div>
        <h1>New Employee</h1>
        <form action={addEmployee}>
            <label>
                Firstname
                <input type="text" name="firstname" required />
            </label>
            <label>
                Lastname
                <input type="text" name="lastname" required />
            </label>
            <label>
                Email
                <input type="email" name="email" required />
            </label>
            <label>
                Password
                <input type="password " name="password" required />
            </label>
            <fieldset>
                <legend>Role</legend>
                <div className="radiobutton">
                    <input type="radio" id="USER" name="role" value="USER" required />
                    <label htmlFor="USER">User</label>
                </div>
                <div className="radiobutton">
                    <input type="radio" id="ADMIN" name="role" value="ADMIN" required />
                    <label htmlFor="ADMIN">Admin</label>
                </div>
            </fieldset>
            <button type="submit">Create New Employee</button>
        </form>
    </div>
}
