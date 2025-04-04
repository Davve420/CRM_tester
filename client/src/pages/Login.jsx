import { useContext } from "react"
import { GlobalContext } from "../GlobalContext.jsx"
import { useNavigate } from "react-router"

export default function Login() {
    const { setUser } = useContext(GlobalContext)
    const navigate = useNavigate()

    async function login(formData) {
        const response = await fetch(`/api/login`, {
            method: "post",
            credentials: 'include',
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                email: formData.get("email"),
                password: formData.get("password")
            })
        })

        const result = await response.json()

        if (response.ok) {
            setUser(result)
            navigate("/")
        } else {
            alert(`Status: ${response.status}\n${result.message}`)
        }
    }

    return <form action={login}>
        <input type="text" name="email" placeholder="Email" required />
        <input type="password" name="password" placeholder="Password" required />
        <button type="submit">Login</button>
    </form>
}
