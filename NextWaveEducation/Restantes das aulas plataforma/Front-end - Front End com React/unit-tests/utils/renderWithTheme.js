import { render } from "@testing-library/react"
import { element } from "prop-types"
import { ThemeProvider } from "styled-components"
import { theme } from "containers/App/AppTheme"

const ThemedComponent = ({ children }) => {
  return <ThemeProvider theme={theme}>{children}</ThemeProvider>;
};


ThemedComponent.propTypes = {
  children: element.isRequired,
};

const renderWithTheme = (ui, options) =>
  render(ui, { wrapper: ThemedComponent, ...options });

export { renderWithTheme };
