import { useContext } from "react"
import { Outlet, useParams } from "react-router"
import { GlobalContext } from "../../GlobalContext.jsx"

export default function CustomerCheck() {
    const { issue_id } = useParams()
    const { user, getLogin } = useContext(GlobalContext)

    async function loginGuest(formData) {
        const response = await fetch(`/api/login/guest`, {
            method: "post",
            credentials: "include",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                email: formData.get("email"),
                chatId: issue_id
            })
        })

        const result = await response.json()

        if (response.ok) {
            getLogin()
        } else {
            alert(result.message)
        }

    }

    if (!user || user.role != "GUEST") {
        return <main id="customerChat">
            <h1>Enter your email to verify access.</h1>
            <form action={loginGuest} id="loginCustomerForm">
                <input type="email" name="email" placeholder="Email" required />
                <button type="submit">Verify</button>
            </form>
        </main>
    }

    return <Outlet />;
}
