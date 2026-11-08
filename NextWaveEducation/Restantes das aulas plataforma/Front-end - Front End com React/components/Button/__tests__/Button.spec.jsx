import React from "react";
import { cleanup } from "@testing-library/react";
import { renderWithTheme } from "unit-tests/utils/renderWithTheme";
import Button from "../Button";

const onClick = jest.fn();

function mountComponent() {
  return renderWithTheme(
    <Button data-testid="test-button" onClick={onClick}>
      teste
    </Button>
  );
}

afterEach(cleanup);

describe("[components] Button", () => {
  const { container, getByTestId } = mountComponent();

  it("should render Button", () => {
    expect(container).toBeTruthy();
    expect(container).toHaveTextContent("teste");

    expect(getByTestId("test-button")).toHaveStyle("font-weight: bold");
    expect(getByTestId("test-button")).toHaveStyle("height: 40px");
    expect(getByTestId("test-button")).toHaveStyle("border-radius: 40px");
    expect(getByTestId("test-button")).toHaveStyle("text-transform: inherit");
  });
});
